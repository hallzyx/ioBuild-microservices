# IoBuild — Visión Arquitectónica Actual

> **Estado del documento:** Refleja el estado real del sistema validado contra el código fuente.  
> Para la historia de cómo llegamos aquí, ver [`../iterations/`](../iterations/).

---

## 1. Visión General del Sistema

IoBuild es una plataforma de gestión de propiedades con monitoreo IoT. El sistema opera como una arquitectura de microservicios con **6 servicios de dominio** más un API Gateway y una capa de infraestructura IoT.

### Diagrama de Contenedores (C4)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              INTERNET                                        │
│                        iobuild-v2.arroz.dev                                  │
└────────────────────────────────┬───────────────────────────────────────────┘
                                 │ HTTPS :443
                                 ▼
                         ┌──────────────┐
                         │  CLOUDFLARE  │ TLS Flexible + DNS proxied
                         └──────┬───────┘
                                │ HTTP :80 → Azure VM
                                ▼
                    ┌───────────────────────┐
                    │  FRONTEND (Nginx :80)  │ SPA Vue 3 + proxy inverso
                    │  /      → index.html   │
                    │  /api/* → gateway:8080 │
                    └───────────┬───────────┘
                                │ /api/v1/*
                                ▼
          ┌─────────────────────────────────────────────┐
          │            API GATEWAY (YARP :8080)          │
          │                                              │
          │  /api/v1/authentication/* → IAM :5001        │
          │  /api/v1/users/*          → IAM :5001        │
          │  /api/v1/profiles/*       → Profiles :5006   │
          │  /api/v1/devices/*        → Devices :5002    │
          │  /api/v1/projects/*       → Projects :5003   │
          │  /api/v1/units/*          → Projects :5003   │
          │  /api/v1/clients/*        → Projects :5003   │
          │  /api/v1/subscriptions/*  → Subscriptions :5004 │
          │  /api/v1/plans/*          → Subscriptions :5004 │
          │  /api/v1/webhooks/*       → Subscriptions :5004 │
          │  /api/v1/analytics/*      → Analytics :5005  │
          │  GET /health              → estado agregado   │
          └───┬──────┬──────┬──────┬──────┬──────┬──────┘
              │      │      │      │      │      │
              ▼      ▼      ▼      ▼      ▼      ▼
         ┌──────┐┌──────┐┌──────┐┌──────┐┌──────┐┌──────┐
         │ IAM  ││Devic.││Proj. ││Subs. ││Analy.││Prof. │
         │:5001 ││:5002 ││:5003 ││:5004 ││:5005 ││:5006 │
         └──┬───┘└──┬───┘└──┬───┘└──┬───┘└──┬───┘└──┬───┘
            │       │       │       │       │       │
            ▼       ▼       ▼       ▼       ▼       ▼
       ┌────────────────────────────────────────────────┐
       │                   MySQL :3306                   │
       │  iobuild_iam │ _devices │ _projects │ _subs    │
       │  iobuild_analytics │ iobuild_profiles          │
       └────────────────────────────────────────────────┘

              Devices también conecta a:
              ┌──────────────┐   MQTT   ┌──────────────┐
              │ IoT Simulator│──────────▶│  Mosquitto   │
              │ (Python dev) │          │  :1883       │
              └──────────────┘          └──────┬───────┘
                                               │ Subscribe
                                        ┌──────▼───────┐   Write   ┌──────────┐
                                        │ Telemetry    │──────────▶│ InfluxDB │
                                        │ Worker (.NET)│           │  :8086   │
                                        └──────────────┘           └──────────┘

Infraestructura transversal (todos los servicios):
  RabbitMQ  — bus de eventos de dominio (Topic Exchange `iobuild.domain.events`, Transactional Outbox)
  Redis     — token blacklist distribuido (revocación JWT entre réplicas de IAM)
  Jaeger    — tracing distribuido (OpenTelemetry OTLP), UI expuesta en watcher.arroz.dev
```

---

## 2. Bounded Contexts

| Servicio | Puerto | Bounded Context | Responsabilidad |
|----------|--------|-----------------|----------------|
| **IoBuild.IAM** | 5001 | Identidad y Acceso | Auth JWT, registro, logout, revocación de tokens (blacklist distribuido en Redis) |
| **IoBuild.Devices** | 5002 | Gestión de Dispositivos IoT | CRUD dispositivos + pipeline telemetría MQTT→InfluxDB |
| **IoBuild.Projects** | 5003 | Gestión de Proyectos | CRUD proyectos, unidades, clientes |
| **IoBuild.Subscriptions** | 5004 | Suscripciones y Pagos | Planes, Stripe, webhooks, Outbox Pattern, Idempotency Keys |
| **IoBuild.Analytics** | 5005 | Analítica | Dashboards y métricas de uso |
| **IoBuild.Profiles** | 5006 | Perfiles de Usuario | CRUD perfiles, segundo email, ACL facade |
| **IoBuild.Gateway** | 8080 | API Gateway | Routing, health checks agregados, CORS |
| **IoBuild.Shared** | — | Librería transversal | JWT, BCrypt, middleware, interfaces base, convenciones EF |

---

## 3. Arquitectura Interna de Cada Microservicio

Cada microservicio implementa **Clean Architecture en 4 capas**, con la regla de dependencia apuntando hacia el dominio:

```
┌─────────────────────────────────────────────┐
│  INTERFACES (API Layer)                      │
│  Controllers · Resources (DTOs) · Assemblers │
│  Responsabilidad: traducir HTTP ↔ Commands   │
├─────────────────────────────────────────────┤
│  APPLICATION (Service Layer)                 │
│  CommandServices · QueryServices · ACL/Facades│
│  Responsabilidad: orquestar casos de uso     │
├─────────────────────────────────────────────┤
│  DOMAIN (Business Logic)                     │
│  Aggregates · Value Objects · Commands       │
│  Queries · Repository Interfaces · Services  │
│  Responsabilidad: reglas de negocio puras    │
├─────────────────────────────────────────────┤
│  INFRASTRUCTURE (Data Access + Externos)     │
│  EF Core DbContext · Repositories           │
│  Stripe · JWT · BCrypt · InfluxDB · MQTT    │
│  Responsabilidad: persistencia y externos    │
└─────────────────────────────────────────────┘
```

**Regla de dependencia:** `Interfaces → Application → Domain ← Infrastructure`  
El Domain no conoce EF Core, Stripe, ni ningún framework externo.

---

## 4. Patrones Implementados

| Patrón | Dónde | Propósito |
|--------|-------|-----------|
| **API Gateway** | `IoBuild.Gateway` (YARP) | Único punto de entrada, CORS centralizado, health checks agregados |
| **Service Layer (Facade)** | `Application/CommandServices/` | Orquesta casos de uso aislando la lógica de la capa HTTP |
| **CQRS** | `CommandServices` + `QueryServices` | Separa escritura de lectura en cada microservicio |
| **Repository** | `Domain/Repositories/` + `Infrastructure/.../Repositories/` | Abstrae el acceso a datos detrás de interfaces |
| **Unit of Work** | `IUnitOfWork` | Transacciones atómicas (todo o nada) |
| **Chain of Responsibility** | `RequestAuthorizationMiddleware` (IAM) | Pipeline de auth: AllowAnonymous → blacklist → JWT → controller |
| **Strategy** | `ITokenService` + `IHashingService` | Algoritmos intercambiables (JWT, BCrypt) |
| **Adapter** | `*Assembler.cs` | Convierte Resources ↔ Commands ↔ Entities sin acoplar capas |
| **Decorator** | `GlobalExceptionHandlerMiddleware` | Envuelve el pipeline con manejo global de errores |
| **Aggregate Root** | `User`, `Project`, `Device`, `Subscription`, `Profile` | Entidades con lógica de dominio encapsulada |
| **Background Service** | `TelemetryWorker` (Devices), `OutboxWorker` (IAM/Devices/Projects/Subscriptions), consumidores RabbitMQ | Procesos asíncronos long-running en el mismo proceso |
| **Transactional Outbox** | `OutboxWorker` + `OutboxMessage` en IAM, Devices, Projects y Subscriptions | Garantiza que eventos de dominio (pago activado, dispositivo creado, dueño vinculado) no se pierdan |
| **Idempotency Keys** | `IdempotencyKey` (Subscriptions) | Evita cobros duplicados en webhooks de Stripe |
| **Anti-Corruption Layer (ACL)** | `ProfilesContextFacade`, `Analytics/ACL/` | Aísla la comunicación entre bounded contexts |
| **Dependency Injection** | `Program.cs` en todos los servicios | Inversión de dependencias vía interfaces (SOLID DIP) |

---

## 5. Infraestructura IoT

El pipeline de telemetría IoT es asíncrono y no interfiere con las operaciones REST del microservicio Devices:

```
Simulador Python → MQTT QoS 1 → Mosquitto :1883
                                      │
                              Subscribe (telemetry/#)
                                      │
                              TelemetryWorker (.NET BackgroundService)
                                      │
                              InfluxDB OSS 2.7 :8086
                              Bucket: iobuild-telemetry | Retención: 7 días
                                      │
                              GET /api/v1/devices/{id}/energy
                              GET /api/v1/devices/{id}/status
```

**¿Por qué MQTT y no REST?** Los dispositivos IoT envían datos a alta frecuencia (cada 5s). MQTT QoS 1 garantiza entrega sin sobrecargar el microservicio Devices con requests HTTP síncronos.

**¿Por qué InfluxDB?** MySQL no está optimizado para series temporales. InfluxDB comprime datos temporales ~30-50× y tiene queries nativas de rango (`aggregateWindow`). [Ver Iteración 2 para la justificación completa](../iterations/iteration-2-pipeline-iot.md).

---

## 6. Flujo de Pagos (Iteración 3)

El flujo de suscripciones garantiza consistencia ACID con el patrón Outbox:

```
Usuario → POST /subscriptions/payments/create-session
               │
               ▼
       SubscriptionsService
               │
       Stripe.CreateCheckoutSession()
               │
               ▼
       Usuario → Stripe Checkout UI → Pago exitoso
               │
       Stripe → POST /api/v1/webhooks/payment (firmado)
               │
       SubscriptionsService.ConfirmPayment()
          ├── Activar suscripción (MySQL, ACID)
          └── Insertar OutboxMessage ("subscription.activated")
               │
       OutboxWorker (cada 5s)
          └── Procesar mensajes pendientes → marcar como "Processed"

Idempotency Keys evitan procesar el mismo webhook dos veces.
```

---

## 7. Decisiones Arquitectónicas (ADRs)

| ADR | Decisión | Estado |
|-----|----------|--------|
| **ADR-01** | API Gateway con YARP | ✅ Implementado |
| **ADR-02** | IAM como microservicio separado | ✅ Implementado |
| **ADR-03** | Mosquitto MQTT como broker IoT | ✅ Implementado |
| **ADR-04** | Persistencia políglota (MySQL + InfluxDB) | ✅ Implementado |
| **ADR-05** | TelemetryWorker in-process (BackgroundService) | ✅ Implementado |
| **ADR-06** | PointData API para escritura en InfluxDB (no WriteMeasurement) | ✅ Implementado |
| **ADR-07** | Simulador IoT en Python Alpine (herramienta de dev, no producción) | ✅ Implementado |
| **ADR-08** | Tres capas de proxy: Cloudflare + Nginx + YARP | ✅ Implementado |
| **ADR-09** | Transactional Outbox Pattern para eventos de pago | ✅ Implementado |
| **ADR-10** | Idempotency Keys en webhooks Stripe | ✅ Implementado |
| **ADR-11** | IoBuild.Shared como Class Library (no NuGet) | ✅ Implementado |
| **ADR-12** | MySQL por servicio (6 contenedores independientes, no instancia compartida) | ✅ Implementado — ver [MS-03](../migrations/support-migrations.md#ms-03-mysql-por-servicio-database-per-service-real) |
| **ADR-13** | OpenTelemetry con auto-instrumentación (ASP.NET Core + HttpClient) en los 7 microservicios | ✅ Implementado — Iteración 4 |
| **ADR-14** | Jaeger All-in-One sin almacenamiento persistente (memoria efímera) | ✅ Implementado — Iteración 4 |
| **ADR-15** | Topic Exchange único compartido (`iobuild.domain.events`) para eventos de dominio entre bounded contexts | ✅ Implementado — Iteración 5 |
| **ADR-16** | Idempotencia por pre-check + índice único (backstop) en cada consumidor de dominio | ✅ Implementado — Iteración 5 |

Para la justificación detallada de cada ADR, ver los [reportes de iteración](../iterations/).

---

## 8. Stack Tecnológico

| Capa | Tecnología |
|------|-----------|
| **Runtime** | .NET 9 (ASP.NET Core) |
| **Base de datos relacional** | MySQL 8.0 |
| **ORM** | Entity Framework Core |
| **Base de datos de series temporales** | InfluxDB OSS 2.7 |
| **Message Broker (IoT)** | Eclipse Mosquitto (MQTT) |
| **Message Broker (eventos de dominio)** | RabbitMQ 4 (Topic Exchange, Transactional Outbox) |
| **Cache / Token Blacklist** | Redis (revocación JWT distribuida) |
| **Tracing distribuido** | OpenTelemetry (auto-instrumentación) + Jaeger All-in-One |
| **API Gateway** | YARP (Yet Another Reverse Proxy) |
| **Proxy Edge** | Cloudflare (TLS + DNS) + Nginx (SPA/reverse proxy) — reemplazaron Traefik + Dokploy de la Iteración 1 |
| **Frontend** | Vue 3 (SPA) + Nginx |
| **Autenticación** | JWT (HMAC-SHA256) + BCrypt |
| **Pagos** | Stripe (Checkout + Webhooks) |
| **Contenerización** | Docker + Docker Compose |
| **CI/CD (Build)** | GitHub Actions → GHCR (imágenes públicas) |
| **Deploy** | Terraform (Azure VM) + Cloudflare (DNS + TLS) |
| **API Docs** | Swagger / OpenAPI |
| **Testing** | xUnit + Moq + FluentAssertions + SpecFlow + Integration Tests (Bash/curl) |
| **IoT Simulator** | Python 3.12 Alpine + paho-mqtt |

---

## 9. Resumen de Tests

| Tipo | Iteración | Cantidad | Estado |
|------|-----------|:--------:|:------:|
| BDD Scenarios (SpecFlow) | 1 | 16 | ✅ 16/16 |
| BDD Scenarios (SpecFlow) | 2 | 4 | ✅ 4/4 |
| Unit Tests (xUnit) | 2 | 22 | ✅ 22/22 |
| Unit Tests — Outbox/Idempotency | 3 | 4+ | ✅ Pasando |
| Integration Tests (Bash/curl) | 1 | 10 | ✅ 10/10 |
| Runtime Tests (curl) | 2 | 10 | ✅ 10/10 |

---

> Para entender la evolución arquitectónica, leer los reportes de iteración en orden:
> 1. [Iteración 1 — Base y Seguridad](../iterations/iteration-1-base-seguridad.md)
> 2. [Iteración 2 — Pipeline IoT](../iterations/iteration-2-pipeline-iot.md)
> 3. [Iteración 3 — Pagos Seguros](../iterations/iteration-3-pagos-seguros.md)
> 4. [Iteración 4 — Observabilidad y Trazabilidad Distribuida](../iterations/iteration-4-observabilidad.md)
> 5. [Iteración 5 — Comunicación Asíncrona entre Bounded Contexts](../iterations/iteration-5-eventos-dominio.md)
