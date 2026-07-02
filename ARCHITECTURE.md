# IoBuild — Arquitectura Actual (As-Built)

> Este documento resume **cómo es el sistema hoy**, validado contra el código y el `docker-compose` de producción.
> Para la historia de cómo se llegó hasta acá, ver los documentos históricos: [`ADD_Iteration_1/2/3_IoBuild.md`](ADD_Iteration_1_IoBuild.md) (diseño por iteración) y [`README.md`](README.md) (narrativa monolito → microservicios).
> Para el detalle técnico completo por capa, ver [`microservices/docs/architecture/overview.md`](microservices/docs/architecture/overview.md).

---

## 1. Visión General

IoBuild es una plataforma de gestión de propiedades con monitoreo IoT, operando como **6 microservicios de dominio** + API Gateway + capa de infraestructura de eventos, tracing e IoT.

### Diagrama de Contenedores

```
INTERNET → Cloudflare (TLS + DNS) → Azure VM
                                         │
                              Frontend (Nginx :80) — SPA Vue 3
                                         │ /api/*
                                         ▼
                          API GATEWAY (YARP :8080)
        /api/v1/authentication/*  → IAM :5001
        /api/v1/users/*           → IAM :5001
        /api/v1/profiles/*        → Profiles :5006
        /api/v1/devices/*         → Devices :5002
        /api/v1/projects|units|clients/* → Projects :5003
        /api/v1/subscriptions|plans|webhooks/* → Subscriptions :5004
        /api/v1/analytics/*       → Analytics :5005
        GET /health                → estado agregado
                                         │
      ┌───────┬───────┬───────┬───────┬───────┬───────┐
      ▼       ▼       ▼       ▼       ▼       ▼       ▼
    IAM    Devices  Projects  Subs  Analytics Profiles
   :5001    :5002    :5003   :5004   :5005    :5006
      │       │        │       │       │        │
      ▼       ▼        ▼       ▼       ▼        ▼
  MySQL-iam MySQL-dev MySQL-proj MySQL-subs MySQL-analytics MySQL-profiles
  (contenedor propio por servicio, puertos 3307-3312)

Infraestructura transversal:
  RabbitMQ  — bus de eventos (Outbox Pattern, ej. subscription.activated)
  Redis     — token blacklist distribuido (revocación JWT entre réplicas)
  Jaeger    — tracing distribuido (OpenTelemetry, expuesto en watcher.arroz.dev)
  Mosquitto → TelemetryWorker (Devices) → InfluxDB — pipeline MQTT de IoT
```

---

## 2. Bounded Contexts

| Servicio | Puerto | Responsabilidad |
|----------|--------|-----------------|
| **IAM** | 5001 | Auth JWT, registro, logout, revocación de tokens (Redis) |
| **Devices** | 5002 | CRUD dispositivos + pipeline telemetría MQTT → InfluxDB |
| **Projects** | 5003 | Proyectos, pisos/unidades, clientes constructores |
| **Subscriptions** | 5004 | Planes, Stripe, webhooks, Outbox (RabbitMQ), Idempotency Keys |
| **Analytics** | 5005 | Dashboards, métricas de energía en vivo |
| **Profiles** | 5006 | Perfiles de usuario, segundo email, ACL facade |
| **Gateway** | 8080 | Routing (YARP), health checks agregados, CORS |
| **IoBuild.Shared** | — | Librería transversal: JWT, BCrypt, middleware, convenciones EF Core |

---

## 3. Infraestructura

| Componente | Rol | Detalle |
|-----------|-----|---------|
| **MySQL** (×6) | Persistencia relacional | Un contenedor por servicio (no compartido), puertos 3307-3312 en dev |
| **InfluxDB** | Series temporales | Telemetría IoT, retención 7 días |
| **Mosquitto** | Broker MQTT | Ingesta de eventos de dispositivos IoT |
| **RabbitMQ** | Broker de eventos | Transactional Outbox Pattern (ej. activación de suscripciones) |
| **Redis** | Cache distribuido | Blacklist de tokens JWT — reemplazó la blacklist en memoria para soportar múltiples réplicas |
| **Jaeger** | Tracing distribuido | OpenTelemetry (OTLP), UI expuesta en `watcher.arroz.dev` |
| **Cloudflare + Nginx + YARP** | Tres capas de proxy | TLS edge → SPA/reverse proxy → API Gateway |

---

## 4. Patrones Implementados

API Gateway · Service Layer (Facade) · CQRS · Repository · Unit of Work · Chain of Responsibility (middleware) · Strategy (JWT/BCrypt) · Adapter (Assemblers) · Decorator (manejo global de excepciones) · Aggregate Root · Background Service (`TelemetryWorker`, `OutboxWorker`) · **Transactional Outbox** (RabbitMQ) · Idempotency Keys (webhooks Stripe) · Anti-Corruption Layer (`*ContextFacade`) · Dependency Injection.

Detalle de dónde vive cada patrón en el código: [`microservices/docs/architecture/overview.md#4-patrones-implementados`](microservices/docs/architecture/overview.md).

---

## 5. Stack Tecnológico

.NET 9 · MySQL 8 · EF Core · InfluxDB OSS 2.7 · Mosquitto (MQTT) · RabbitMQ 4 · Redis · YARP · Vue 3 + Nginx · JWT (HMAC-SHA256) + BCrypt · Stripe · Docker + Docker Compose · GitHub Actions → GHCR · Terraform (Azure VM) + Cloudflare · OpenTelemetry + Jaeger · xUnit + Moq + FluentAssertions + SpecFlow.

---

## 6. Cómo evolucionó

El sistema pasó por 5 iteraciones ADD: base + seguridad (QA-1) → pipeline IoT (QA-2) → pagos seguros (QA-3) → observabilidad y tracing distribuido (QA-4) → comunicación asíncrona entre bounded contexts vía RabbitMQ (QA-5/US40/US41/CRN-5). El único cambio de infraestructura que sigue sin driver propio es el split de MySQL a un contenedor por servicio — ver [`microservices/docs/migrations/support-migrations.md`](microservices/docs/migrations/support-migrations.md). El detalle de las Iteraciones 4 y 5 está en [`microservices/docs/iterations/`](microservices/docs/iterations/).
