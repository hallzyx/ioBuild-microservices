# IoBuild — Iteration 3 Final Report

## Gestión de Suscripciones, Planes y Pagos Seguros

**Proyecto:** IoBuild — Sistema de Gestión de Propiedades e IoT  
**Curso:** Fundamentos de Arquitectura de Software — UPC  
**Iteración:** 3  
**Fecha:** Junio 2026  
**Estado:** Implementado ✅

---

## Índice

1. [Objetivo de la Iteración](#1-objetivo-de-la-iteración)
2. [Drivers Arquitectónicos](#2-drivers-arquitectónicos)
3. [Decisiones Arquitectónicas (ADRs)](#3-decisiones-arquitectónicas-adrs)
4. [Componentes Implementados](#4-componentes-implementados)
5. [Flujo de Pago Completo](#5-flujo-de-pago-completo)
6. [Resultados de Testing](#6-resultados-de-testing)
7. [Evolución desde Iteración 2](#7-evolución-desde-iteración-2)

---

## 1. Objetivo de la Iteración

La Iteración 2 estableció el pipeline de telemetría IoT. El sistema ya podía recibir datos de dispositivos y mostrar dashboards en tiempo real. Pero el modelo de negocio de IoBuild depende de **suscripciones**: los constructores pagan por acceso al sistema.

**El problema:** La Iteración 1 tenía integración básica con Stripe (crear sesión de checkout y confirmar manualmente), pero sin garantías de consistencia. Si la red fallaba entre que Stripe notificaba el pago y el sistema activaba la suscripción, el usuario quedaba con un cobro en su tarjeta pero sin acceso a la plataforma.

**El objetivo de esta iteración:**
> *"Diseñar un mecanismo seguro, idempotente y consistente para procesar pagos y gestionar el ciclo de vida de las suscripciones, asegurando que los usuarios reciban los beneficios de su plan inmediatamente después del cobro exitoso."*

---

## 2. Drivers Arquitectónicos

| ID | Tipo | Descripción | Cobertura |
|:--:|:----:|------------|:---------:|
| **QA-3** | Quality Attribute | **Confiabilidad / Consistencia:** Al confirmar un pago, el sistema procesa el cobro y actualiza la suscripción de forma atómica. Si ocurre un fallo de red, no se pierde la confirmación ni se duplican cobros. | ✅ Outbox Pattern + Idempotency Keys |
| **US28** | Primary Functionality | Ver el plan de suscripción actual y su estado para confirmar los beneficios que se tienen. | ✅ `GET /api/v1/subscriptions` + `GET /api/v1/plans` |
| **US31** | Primary Functionality | Renovar el plan activo para asegurar la continuidad del servicio. | ✅ `POST /api/v1/subscriptions/payments/create-session` + webhook confirm |
| **CON-1** | Constraint | El backend debe implementarse bajo un enfoque de microservicios. | ✅ Todo en `IoBuild.Subscriptions` sin acoplamiento a otros servicios |

---

## 3. Decisiones Arquitectónicas (ADRs)

### ADR-09: Transactional Outbox Pattern

**Decisión:** Al confirmar un pago en el webhook de Stripe, la activación de la suscripción y la inserción de un `OutboxMessage` ocurren en la **misma transacción de base de datos** (ACID).

**Racional:** Sin el Outbox, si el proceso caía entre actualizar la suscripción y notificar al resto del sistema, se perdía el evento. Con Outbox, el `OutboxWorker` reintenta la entrega hasta completar.

**Trade-off:** Complejidad adicional (tabla `OutboxMessages`, worker background). Mitigado: el worker usa backoff simple (5s entre ciclos, máx 3 reintentos) y es fácil de monitorear con logs.

### ADR-10: Idempotency Keys para Webhooks

**Decisión:** Cada request de creación de pago lleva un `Idempotency-Key` único por intento. El sistema verifica que la clave no haya sido procesada antes de ejecutar la operación.

**Racional:** Stripe puede reenviar el mismo webhook si no recibe confirmación (200 OK) en tiempo. Sin idempotencia, un webhook reenviado duplicaría la activación de la suscripción (o peor, un cobro doble).

**Implementación:** Entidad `IdempotencyKey` con campo `ExpiresAt`. El servicio verifica existencia antes de procesar. Si ya existe → retorna 200 OK inmediatamente (safe to replay).

### Conceptos Descartados

| Concepto | Motivo |
|----------|--------|
| **Fire-and-Forget para confirmación de pago** | Prohibido explícitamente: un fallo de red dejaría al usuario cobrado sin acceso |
| **Consistencia eventual básica** | No aceptable para operaciones de dinero — se exige ACID en la BD de suscripciones |
| **RabbitMQ para el Outbox** | Overkill para la escala actual. El `OutboxWorker` en el mismo proceso es suficiente |

---

## 4. Componentes Implementados

### 4.1 OutboxWorker (BackgroundService)

**Ubicación:** `src/IoBuild.Subscriptions/Workers/OutboxWorker.cs`

El worker corre en el mismo proceso que la API de Subscriptions. Cada 5 segundos:
1. Lee mensajes en estado `"Pending"` desde la tabla `OutboxMessages`
2. Los procesa (marca como `"Processed"`)
3. Si falla: incrementa `RetryCount`. Al llegar a 3 intentos fallidos → estado `"Failed"`

```
OutboxWorker (cada 5s)
    │
    └── GetPendingAsync() → lista de OutboxMessages
             │
             ├── ProcesseAsync(msg) → "Processed" + ProcessedAt
             │
             └── on error → RetryCount++ → "Failed" si retries >= 3
```

**Tolerancia a fallos:** Si el worker falla en un ciclo, el siguiente ciclo reintenta. La API de Subscriptions no se ve afectada (son hilos separados del mismo proceso).

### 4.2 IdempotencyKey

**Entidad:** `src/IoBuild.Subscriptions/Domain/Model/Entities/IdempotencyKey.cs`

| Campo | Descripción |
|-------|-------------|
| `Key` | Clave única (e.g. `"renew_1_2"` para builder 1, plan 2) |
| `CreatedAt` | Timestamp de creación |
| `ExpiresAt` | TTL (24h por defecto) |

Flujo: antes de procesar cualquier operación de pago, el servicio busca si la clave existe y no expiró. Si existe → retorna resultado anterior. Si no → procesa y persiste la clave.

### 4.3 OutboxMessage

**Entidad:** `src/IoBuild.Subscriptions/Domain/Model/Entities/OutboxMessage.cs`

| Campo | Descripción |
|-------|-------------|
| `EventType` | Tipo de evento (e.g. `"subscription.activated"`) |
| `Payload` | JSON con datos del evento |
| `Status` | `"Pending"` → `"Processed"` / `"Failed"` |
| `RetryCount` | Contador de reintentos |
| `CreatedAt` / `ProcessedAt` | Timestamps |
| `Error` | Mensaje de error en caso de fallo |

### 4.4 StripePaymentService

**Ubicación:** `src/IoBuild.Subscriptions/Infrastructure/Payment/Stripe/Services/StripePaymentService.cs`

Implementa la integración con Stripe Checkout:
- `CreateCheckoutSession()` — crea la sesión de pago con metadata `builder_id` + `plan_id`
- El webhook de Stripe llama a `POST /api/v1/webhooks/payment` con firma criptográfica
- El servicio verifica la firma antes de procesar el evento

### 4.5 Gateway — Nuevas Rutas

Se agregó el route `/api/v1/webhooks/*` apuntando al cluster de Subscriptions:

```json
"webhooks-all": {
  "ClusterId": "subscriptions-cluster",
  "Match": { "Path": "/api/v1/webhooks/{**catch-all}" }
}
```

**¿Por qué un path separado para webhooks?** Stripe envía los webhooks a una URL fija configurada en su dashboard. Tenerla separada de `/subscriptions/payments/` hace la configuración más explícita y permite aplicar middleware específico (verificación de firma Stripe) solo en ese path.

---

## 5. Flujo de Pago Completo

### Secuencia UC Crítico — US31 (Renovar Plan)

```
Property Manager
    │
    ├── POST /api/v1/subscriptions/payments/create-session
    │       { planId: 2, builderId: 1, idempotencyKey: "renew_1_2" }
    │
    ▼
SubscriptionsService
    ├── Verificar IdempotencyKey (¿ya procesado? → return early)
    ├── Stripe.CreateCheckoutSession(planId, builderId)
    └── return { checkoutUrl: "https://checkout.stripe.com/..." }
    
Property Manager → abre URL → paga con tarjeta → Stripe confirma
    │
    ▼
Stripe → POST /api/v1/webhooks/payment (firmado con webhook secret)
    │
    ▼
WebhooksController
    ├── Verificar firma criptográfica de Stripe
    └── SubscriptionsService.ConfirmPayment(sessionId)
            │
            ├── [Transacción ACID]
            │   ├── Actualizar Subscription.Status → "active"
            │   ├── Subscription.EndDate → ahora + 1 mes
            │   └── INSERT OutboxMessage("subscription.activated", {...})
            │
            └── return 200 OK (Stripe deja de reintentar el webhook)
    
OutboxWorker (5s después)
    └── Procesar OutboxMessage → "Processed"
```

### Flujo de Seguridad del Webhook

```
Stripe envía → POST /api/v1/webhooks/payment
                      │
              StripePaymentService.VerifySignature(payload, signature, secret)
                      │
              ¿Firma válida? → NO → 400 Bad Request (log de intento)
                      │
              ¿Firma válida? → SÍ → procesar evento
```

La firma usa HMAC-SHA256 con el webhook secret configurado en `StripeSettings`. Sin esto, cualquier tercero podría simular un webhook de "pago exitoso".

---

## 6. Resultados de Testing

### Unit Tests — OutboxPaymentTests

Archivo: `tests/IoBuild.Subscriptions.Tests/OutboxPaymentTests.cs`

| Test | Qué verifica |
|------|-------------|
| `OutboxMessage_Creation_SetsProperties` | EventType, Payload, Status="Pending", RetryCount=0 |
| `OutboxMessage_DefaultsAreCorrect` | Defaults correctos + CreatedAt <= ahora |
| `IdempotencyKey_Creation_SetsProperties` | Key, CreatedAt, ExpiresAt > ahora |

### BDD Scenarios (Iteración 1 — vigentes para Subscriptions)

Los 4 escenarios de `SubscriptionRenewal.feature` definidos en Iteración 1 siguen pasando y cubren:
- Crear sesión de pago → URL de Stripe
- Confirmar pago exitoso → suscripción activa
- Webhook con pago fallido → suscripción sin cambios
- Estado de suscripción actual (US28)

---

## 7. Evolución desde Iteración 2

| Aspecto | Iteración 2 | Iteración 3 |
|---------|:-----------:|:-----------:|
| **Patrón de pago** | Stripe básico (sin garantías) | **Outbox + Idempotency Keys** |
| **Consistencia de pagos** | Best-effort | **ACID** con Outbox |
| **Riesgo de cobro doble** | Presente | **Eliminado** con Idempotency Keys |
| **Webhook** | `POST /subscriptions/payments/confirm` (manual) | `POST /webhooks/payment` (Stripe-signed) |
| **Workers en Subscriptions** | — | **OutboxWorker** (BackgroundService) |
| **Rutas nuevas en Gateway** | — | `/api/v1/webhooks/*` |

---

> **Documento generado para el curso de Fundamentos de Arquitectura de Software — UPC**  
> **Proyecto:** IoBuild — Iteración 3 (Cierre)  
> **Fecha:** Junio 2026  
> **Estado:** ✅ Implementado — OutboxWorker + Idempotency Keys + Webhook firmado
