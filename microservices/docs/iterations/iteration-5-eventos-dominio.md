# IoBuild — Reporte Final de la Iteración 5

## Comunicación Asíncrona entre Bounded Contexts (RabbitMQ Event Bus)

**Proyecto:** IoBuild — Sistema de Gestión de Propiedades e IoT
**Curso:** Fundamentos de Arquitectura de Software — UPC
**Iteración:** 5
**Estado:** Implementado ✅

---

## Índice

1. [Objetivo de la Iteración](#1-objetivo-de-la-iteración)
2. [Drivers Arquitectónicos](#2-drivers-arquitectónicos)
3. [Decisiones Arquitectónicas (ADRs)](#3-decisiones-arquitectónicas-adrs)
4. [Componentes Implementados](#4-componentes-implementados)
5. [Flujos End-to-End](#5-flujos-end-to-end)
6. [Resultados de Testing](#6-resultados-de-testing)
7. [Qué NO cubre esta iteración](#7-qué-no-cubre-esta-iteración)
8. [Evolución desde Iteración 4](#8-evolución-desde-iteración-4)

---

## 1. Objetivo de la Iteración

La Iteración 3 introdujo el Transactional Outbox Pattern, pero acotado a un único caso: Subscriptions notificando "plan activado". A medida que el sistema creció, aparecieron casos de uso que requerían el mismo nivel de confiabilidad pero con **varios bounded contexts consumiendo el mismo evento** — algo que un Outbox in-process de un solo consumidor no resuelve.

**El objetivo de esta iteración:**
> *"Extender la comunicación asíncrona entre bounded contexts para soportar aprovisionamiento automático de dispositivos y vinculación de propietarios, sin acoplar directamente a Devices, Projects, IAM y Analytics entre sí."*

---

## 2. Drivers Arquitectónicos

| ID | Tipo | Descripción | Cobertura |
|:--:|:----:|------------|:---------:|
| **QA-5** | Quality Attribute | **Consistencia Eventual entre Bounded Contexts:** un evento publicado es consumido por N servicios sin pérdida ni duplicación, aun si un consumidor está caído al publicarse. | ✅ Outbox por productor + idempotencia por consumidor |
| **US40** | Primary Functionality | Aprovisionamiento automático de dispositivos IoT al definir un piso o unidad. | ✅ `FloorProvisioningConsumer` + `UnitDeviceProvisioningConsumer` |
| **US41** | Primary Functionality | Vinculación automática de propietario a unidad por email al registrarse. | ✅ `OwnerLinkingConsumer` + `UnitOwnerAnnouncer` |
| **CRN-5** | Architectural Concern | Un evento debe soportar múltiples consumidores en distintos bounded contexts sin que el publicador los conozca. | ✅ Topic Exchange con wildcard-bind en Analytics |

---

## 3. Decisiones Arquitectónicas (ADRs)

### ADR-15: Topic Exchange único compartido (`iobuild.domain.events`)

**Decisión:** Todos los productores (IAM, Devices, Projects, Subscriptions) publican al mismo exchange topic `iobuild.domain.events`; cada consumidor declara su propia cola y hace *bind* solo a los routing keys que le interesan.

**Racional:** Analytics necesita eventos de Devices y de Projects a la vez (`device.#`, `project.#`). Con exchanges separados por bounded context, esto hubiera requerido federación/shovel entre brokers. Un exchange único con routing keys jerárquicas (`project.floor.defined`, `device.device.created`, `iam.user.registered`) resuelve el fan-out sin infraestructura adicional.

**Trade-off:** El nombre del exchange está hardcodeado (`RabbitMqDomainEventPublisher.ExchangeName`) — compartido por diseño, no aislado por servicio.

### ADR-16: Idempotencia por pre-check + índice único (backstop)

**Decisión:** Cada consumidor de dominio (`FloorProvisioningConsumer`, `UnitDeviceProvisioningConsumer`, `OwnerLinkingConsumer`, `UnitOwnerProjectionConsumer`) verifica si el efecto ya se aplicó **antes** de escribir (pre-check), y además tiene un índice único en base de datos como red de seguridad si dos entregas llegan de forma concurrente.

**Racional:** RabbitMQ garantiza *at-least-once delivery*, no *exactly-once*. Un mensaje puede reentregarse tras un fallo de red, un restart del consumidor, o un `Nack` con requeue. Sin idempotencia, una reentrega duplicaría dispositivos aprovisionados o vínculos de dueño.

**Implementación real por consumidor:**

| Consumidor | Pre-check | Backstop (BD) |
|---|---|---|
| `FloorProvisioningConsumer` | `ExistsByProjectFloorTypeAsync` | Índice único `(ProjectId, FloorNumber, Type)` |
| `UnitDeviceProvisioningConsumer` | Count de dispositivos existentes por `(ProjectId, UnitId)` | Índice único filtrado `(ProjectId, UnitId, Type) WHERE unit_id IS NOT NULL` |
| `OwnerLinkingConsumer` | Unidades con `OwnerId IS NULL` únicamente | Upsert `registered_owner` con guard LWW ("last write wins" por timestamp) |
| `UnitOwnerProjectionConsumer` | Existencia de fila `(UnitId, OwnerUserId)` | Insert-or-update sobre la misma clave |

Un `DbUpdateException` por violación de índice único se trata como "ya provisionado" y se responde `Ack` de todas formas — la reentrega no es un error, es el caso esperado.

### Conceptos Descartados

| Concepto | Motivo |
|----------|--------|
| **Exchange separado por bounded context** | Rompía el wildcard-bind de Analytics sin agregar federación entre brokers. |
| **Llamadas HTTP síncronas Projects → Devices para aprovisionar** | Un fallo de red hubiera bloqueado la respuesta de `define-structure` al usuario. |
| **Ack automático sin verificación de firma/esquema** | Se descartó procesar mensajes sin validar el `event-type` en el header AMQP — evita deserializar payloads inesperados como el tipo equivocado. |

---

## 4. Componentes Implementados

### 4.1 Productores (Outbox por servicio)

| Servicio | Evento publicado | Routing Key | Disparado por |
|---|---|---|---|
| Projects | `FloorStructureDefinedEvent` | `project.floor.defined` | `POST /projects/{id}/floors/define-structure` |
| Projects | `UnitDevicesDefinedEvent` | `project.unit.devices.defined` | Selección de paquete de dispositivos por unidad |
| Projects | `UnitOwnerMatchedEvent` | `project.unit.owner-matched` | Match de email al vincular dueño |
| IAM | `UserRegisteredEvent` | `iam.user.registered` | `POST /authentication/sign-up` |
| Devices | `DeviceCreatedEvent` / `DeviceUpdatedEvent` / `DeviceDeletedEvent` | `device.device.*` | CRUD de dispositivos (manual o vía provisioning) |

### 4.2 Consumidores

**`FloorProvisioningConsumer`** (`src/IoBuild.Devices/Infrastructure/Messaging/FloorProvisioningConsumer.cs`)
Cola `devices.provisioning`, bind a `project.floor.defined`. Aprovisiona el set de dispositivos por defecto de un piso en dos fases: (1) inserta los `Device` y confirma sus IDs reales, (2) escribe los `DeviceCreatedEvent` de salida en el Outbox con esos IDs ya asignados.

**`UnitDeviceProvisioningConsumer`** (`src/IoBuild.Devices/Infrastructure/Messaging/UnitDeviceProvisioningConsumer.cs`)
Cola `devices.unit-provisioning`, bind a `project.unit.devices.defined`. Mismo patrón de dos fases, a nivel de unidad en vez de piso.

**`OwnerLinkingConsumer`** (`src/IoBuild.Projects/Infrastructure/Messaging/OwnerLinkingConsumer.cs`)
Cola `projects.owner-linking`, bind a `iam.user.#`. Ante un `UserRegisteredEvent` con `Role == "Owner"`: registra el email, busca unidades con `OwnerEmail` coincidente y `OwnerId` nulo, vincula cada match y escribe `UnitOwnerMatchedEvent` — todo en una sola transacción con `CompleteAsync` atómico.

**`UnitOwnerProjectionConsumer`** (`src/IoBuild.Devices/Infrastructure/Messaging/UnitOwnerProjectionConsumer.cs`)
Cola `devices.unit-owner-projection`, bind a `project.unit.owner-matched`. Mantiene una proyección local (`UnitOwnerProjection`) dentro de Devices para poder validar "¿este usuario es dueño de esta unidad?" sin una llamada HTTP síncrona a Projects en cada comando de dispositivo.

**`AnalyticsEventConsumer`** (`src/IoBuild.Analytics/Infrastructure/Messaging/AnalyticsEventConsumer.cs`)
Cola `analytics.read-model`, bind wildcard a `device.#` y `project.#`. Es el consumidor que mejor demuestra CRN-5: escucha eventos de **dos** bounded contexts distintos sin que ninguno de los dos sepa que Analytics existe.

**`UnitOwnerAnnouncer`** (`src/IoBuild.Projects/Infrastructure/Messaging/UnitOwnerAnnouncer.cs`)
`BackgroundService` de arranque — no consume, **re-publica** `UnitOwnerMatchedEvent` para cada unidad que ya tiene `OwnerId` en la BD. Publica directo (no vía Outbox) porque `UnitOwnerProjectionConsumer` es idempotente por diseño (upsert por `UnitId`), así que reanunciar en cada restart es seguro y evita acumular filas de Outbox. Reconcilia el estado de Devices después de un `docker compose down -v`.

---

## 5. Flujos End-to-End

### 5.1 Aprovisionamiento de dispositivos por piso (US40)

```
Property Manager
    │
    ├── POST /projects/{id}/floors/define-structure
    │
    ▼
Projects Service
    ├── [Transacción ACID] Insert Floor + OutboxMessage(FloorStructureDefinedEvent)
    └── return 201 Created

OutboxWorker (Projects, ~5s después)
    └── Publish → RabbitMQ (routing: project.floor.defined)
              │
              ▼
FloorProvisioningConsumer (Devices)
    ├── Pre-check idempotencia (ProjectId, FloorNumber, Type)
    ├── [Transacción ACID] Insert Devices (Phase 1 — IDs reales asignados)
    ├── [Transacción ACID] Insert OutboxMessage(DeviceCreatedEvent) por device (Phase 2)
    └── Ack

OutboxWorker (Devices)
    └── Publish DeviceCreatedEvent → RabbitMQ → AnalyticsEventConsumer actualiza read-model
```

### 5.2 Vinculación de propietario por email (US41)

```
Usuario se registra con rol "Owner" y el email X
    │
    ▼
IAM Service
    ├── [Transacción ACID] Insert User + OutboxMessage(UserRegisteredEvent)
    └── return 201 Created

OutboxWorker (IAM)
    └── Publish → RabbitMQ (routing: iam.user.registered)
              │
              ▼
OwnerLinkingConsumer (Projects)
    ├── Upsert registered_owner (email lower-cased)
    ├── Query Units WHERE OwnerEmail = X AND OwnerId IS NULL
    ├── Por cada match: Unit.LinkOwner(userId) + OutboxMessage(UnitOwnerMatchedEvent)
    └── Ack (commit único, atómico)

OutboxWorker (Projects)
    └── Publish UnitOwnerMatchedEvent → RabbitMQ
              │
              ├──▶ UnitOwnerProjectionConsumer (Devices) — actualiza proyección local
              └──▶ AnalyticsEventConsumer — actualiza read-model
```

**Caso de reconciliación:** si el usuario se registra **antes** de que la unidad exista con su email asignado, no hay match — el vínculo ocurre recién cuando la unidad se crea con `OwnerEmail = X`. `UnitOwnerAnnouncer` no resuelve este caso (solo re-publica vínculos ya existentes); es un caso de la Iteración 3 original (creación de unidad), no de esta iteración.

---

## 6. Resultados de Testing

| Componente | Archivo de test | Casos |
|---|---|:---:|
| `FloorProvisioningConsumer` | `tests/IoBuild.Devices.Tests/Application/FloorProvisioningConsumerTests.cs` | 14 |
| `UnitDeviceProvisioningConsumer` | `tests/IoBuild.Devices.Tests/Application/UnitDeviceProvisioningConsumerTests.cs` | 8 |
| `UnitOwnerProjectionConsumer` | `tests/IoBuild.Devices.Tests/Application/UnitOwnerProjectionConsumerTests.cs` | 2 |
| `OwnerLinkingConsumer` | `tests/IoBuild.Projects.Tests/Application/OwnerLinkingConsumerTests.cs` | 5 |
| `UnitOwnerAnnouncer` | `tests/IoBuild.Projects.Tests/Infrastructure/UnitOwnerAnnouncerTests.cs` | 4 |
| `AnalyticsEventConsumer` (idempotencia + device name) | `tests/IoBuild.Analytics.Tests/Consumers/` + `Infrastructure/` | 6 |
| Contratos de eventos (`FloorStructureDefinedEvent`, `UserRegisteredEvent`, etc.) | `tests/IoBuild.Shared.Tests/Domain/Model/Events/` | Serialización + routing key por evento |

**Total: 39+ casos** cubriendo idempotencia, routing y contratos de eventos de esta iteración.

---

## 7. Qué NO cubre esta iteración

- **Dead Letter Exchange** — un mensaje mal formado se descarta (`Nack(requeue: false)`) sin quedar disponible para inspección posterior.
- **Propagación de trace context sobre AMQP** — un trace de Jaeger (Iteración 4) no incluye el procesamiento asíncrono de estos consumidores; ver la limitación documentada en [`iteration-4-observabilidad.md`](iteration-4-observabilidad.md#6-cómo-leer-un-trace).
- **`DeviceRegistryAnnouncer`** (registro de dispositivos vía MQTT retenido) — es un mecanismo distinto, sobre Mosquitto en vez de RabbitMQ, de la Iteración 2. No forma parte de este event bus.

---

## 8. Evolución desde Iteración 4

| Aspecto | Iteración 4 | Iteración 5 |
|---------|:-----------:|:-----------:|
| **Alcance de RabbitMQ** | Ninguno (solo tracing) | **Bus de eventos de dominio entre 4 bounded contexts** |
| **Consumidores de dominio** | 0 | **5** (`FloorProvisioningConsumer`, `UnitDeviceProvisioningConsumer`, `OwnerLinkingConsumer`, `UnitOwnerProjectionConsumer`, `AnalyticsEventConsumer`) |
| **Patrón de confiabilidad** | N/A | Outbox (Iteración 3) + idempotencia por pre-check/índice único |
| **Fan-out real (1 evento → N consumidores)** | No aplica | `UnitOwnerMatchedEvent` → Devices **y** Analytics |

---

> **Documento generado para el curso de Fundamentos de Arquitectura de Software — UPC**
> **Proyecto:** IoBuild — Iteración 5 (Eventos de Dominio)
> **Estado:** ✅ Implementado — 5 consumidores de dominio sobre `iobuild.domain.events`
