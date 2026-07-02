# IoBuild — De Monolito a Microservicios

## Transformación Arquitectónica Paso a Paso

**Proyecto:** IoBuild — Sistema de Gestión de Propiedades Inteligentes e IoT

---

## ¿Qué es este repositorio?

Este repositorio documenta y ejecuta la **transformación arquitectónica** de una aplicación web tradicional (monolito + frontend Vue) hacia una **arquitectura de microservicios** en .NET 9, guiada por el método **Attribute-Driven Design (ADD)** en su iteración 1 (Base Arquitectónica y Seguridad).

---

## La Historia Completa

### Fase 0: El Estado Original (Monolito)

El proyecto comenzó en el curso "Aplicaciones Web" con una estructura tradicional:

```
fundamentos_arq/
├── IoBuild-Backend/          ← Monolito .NET
│   └── IoBuilt.API/          ← Un solo proyecto Web API
│       ├── IAM/              ← Autenticación (mezclada)
│       ├── Projects/         ← Proyectos (acoplado)
│       ├── Devices/          ← Dispositivos (sin escalar)
│       ├── Subscriptions/    ← Pagos (frágil)
│       └── Shared/           ← Código común
├── IoBuild-Frontend/         ← Vue App
└── ... (documentación)
```

**Problemas detectados:**
| Síntoma | Causa Raíz |
|---------|-----------|
| Controladores gordos | Lógica de negocio + acceso a datos + HTTP todo mezclado |
| Sin tests | No se podía testear sin BD real (DbContext directo) |
| Sin health checks | Nadie sabía si el sistema estaba vivo |
| Todo en un deploy | Si Devices fallaba, caía Projects también |
| Sin Gateway | El frontend tenía que conocer todos los endpoints |

### Fase 1: La Decisión (ADD Iteración 1)

Aplicamos **ADD** con 4 drivers arquitectónicos:

| Driver | Tipo | ¿Qué implica? |
|--------|------|--------------|
| **CRN-1** | Architectural Concern | Crear estructura base Greenfield |
| **QA-1** | Quality Attribute | Seguridad: login < 2s, JWT, revocación de tokens |
| **CON-1** | Constraint | El backend DEBE ser microservicios Cloud Native |
| **CON-2** | Constraint | El frontend DEBE ser Vue |

**Decisión clave:** No refactorizar el monolito "directamente". **Refactorizar desde una copia** en una nueva estructura paralela.

```
Decisión: Crear microservices/ con proyectos independientes
Razón:     El monolito tenía demasiado acoplamiento
Trade-off: Más trabajo inicial, pero base limpia para escalar
```

### Fase 2: La Ejecución (Lo que construimos)

#### Paso 1 — Librería Compartida (`IoBuild.Shared`)

Antes de crear servicios, creamos los cimientos. `IoBuild.Shared.dll` contiene:

| Componente | Función |
|-----------|---------|
| `IBaseRepository<T>` | Contrato CRUD genérico para todos los repos |
| `IUnitOfWork` | Transacciones atómicas |
| `GlobalExceptionHandlerMiddleware` | Captura errores y responde JSON estandarizado |
| `ModelBuilderExtensions` | SnakeCase + pluralización automática para EF Core |
| `KebabCaseRouteNamingConvention` | Rutas como `/api/v1/authentication/sign-in` |
| `ITokenBlacklistService` | Revocación de JWT en memoria |

**Patrón:** IoBuild.Shared es el *cemento* que une todos los microservicios sin duplicar código.

#### Paso 2 — API Gateway (`IoBuild.Gateway` :8080)

El **punto único de entrada**. Usamos YARP (Yet Another Reverse Proxy) de Microsoft.

```
Cliente → Gateway :8080 → decide a qué microservicio enviar
         → GET /health  → monitorea los 5 servicios cada 10s
```

10 rutas mapeadas, 5 clusters, health checks activos con política ConsecutiveFailures.

#### Paso 3 — IAM Microservice (`IoBuild.IAM` :5001)

El **guardián del sistema**. Todo pasa por él antes de llegar a otros servicios.

- `AuthenticationController`: Sign-in, Sign-up, Logout
- `RequestAuthorizationMiddleware`: Valida JWT + blacklist en cada request
- `TokenService`: JWT con HMAC-SHA256, 7 días, claims {sid, email, role}
- `HashingService`: BCrypt para contraseñas
- `TokenBlacklistService`: Revocación inmediata en logout

**¿Por qué separado?** Porque si la autenticación se cae, que solo se caiga la autenticación. Los proyectos, dispositivos y analytics pueden seguir sirviendo datos en caché.

#### Paso 4 — Projects Microservice (`IoBuild.Projects` :5003)

Proyectos de construcción, unidades y clientes constructores. CRUD completo con CQRS.

- `ProjectsController`: Proyectos
- `UnitsController`: Unidades dentro de proyectos
- `ClientsController`: Clientes constructores con estados de cuenta

**¿Por qué separado?** Porque Projects tiene datos relacionales complejos (proyecto → unidades → clientes). Tenerlo aislado permite hacer consultas pesadas sin afectar auth o devices.

#### Paso 5 — Devices Microservice (`IoBuild.Devices` :5002)

Dispositivos IoT y logs de telemetría. Preparado para escalar horizontalmente cuando llegue alta carga de sensores.

- `DevicesController`: CRUD de dispositivos
- `DeviceLogRepository`: Consultas por rango de fechas, tipos, proyecto

**¿Por qué separado?** Porque en IoT, la carga de escritura es enorme (cientos de miles de eventos por minuto). Este servicio necesita escalar independientemente del resto.

#### Paso 6 — Subscriptions Microservice (`IoBuild.Subscriptions` :5004)

Planes, suscripciones y pagos vía Stripe. El servicio más sensible del sistema (maneja dinero).

- `PlansController`: Catálogo de planes
- `SubscriptionsController`: CRUD suscripciones
- `PaymentsController`: Create-session + Confirm (webhook)
- `StripePaymentService`: Integración con Stripe

**¿Por qué separado?** Porque los pagos requieren ACID estricto. Si hay un fallo, no queremos que afecte a proyectos o dispositivos. Aislamos el riesgo.

#### Paso 7 — Analytics Microservice (`IoBuild.Analytics` :5005)

Dashboards y métricas consolidadas de todo el sistema.

- `AnalyticsController`: GET /metrics (dashboard) + GET /insights (históricos)
- `DevicesContextFacade`: Consulta Devices vía HTTP
- `ProjectsContextFacade`: Consulta Projects vía HTTP

**¿Por qué separado?** Las consultas analíticas son pesadas (agregaciones, rangos de tiempo). No queremos que degraden el rendimiento de los servicios transaccionales.

### Fase 3: El Resultado

> **Nota:** el diagrama de abajo muestra el resultado de la Iteración 1 (5 servicios + Gateway). El sistema evolucionó desde entonces — ver [`ARCHITECTURE.md`](ARCHITECTURE.md) para el estado actual (7 servicios de dominio + RabbitMQ + Redis + Jaeger + pipeline IoT completo).

```
Arquitectura Final (Iteración 1)
═════════════════════════════════

Cliente (Vue App :5173)
  │
  ▼
┌──────────────────────────────────────────────────┐
│          API GATEWAY (YARP) :8080                │
│  ┌─────────────────────────────────────────────┐ │
│  │ /api/v1/authentication/*  → IoBuild.IAM     │ │
│  │ /api/v1/users/*           → IoBuild.IAM     │ │
│  │ /api/v1/devices/*         → IoBuild.Devices │ │
│  │ /api/v1/projects/*        → IoBuild.Projects│ │
│  │ /api/v1/subscriptions/*   → IoBuild.Subs    │ │
│  │ /api/v1/analytics/*       → IoBuild.Analyt. │ │
│  │                            ┌──────────┐     │ │
│  │ GET /health → 5/5 Healthy  │ Shared   │     │ │
│  └────────────────────────────┤  Library │     │ │
│                               └──────────┘     │ │
└────────────────────────────────────────────────┘─┘
  │       │       │        │          │
 :5001   :5002   :5003    :5004      :5005
  ▼       ▼       ▼        ▼          ▼
 IAM    Devices  Projects Subs     Analytics
 Auth    IoT     Const.   Stripe   Dashboards
 JWT     Logs    Clientes Planes   Métricas
 BCrypt
 Blacklist

 CADA UNO con su propia BD MySQL
```

---

## Los Patrones (11 en total)

| Patrón | ¿Dónde? | ¿Qué problema resuelve? |
|--------|---------|------------------------|
| **API Gateway** | IoBuild.Gateway | El frontend necesita UN solo punto de entrada |
| **Dependency Injection** | Todos los Program.cs | Desacoplar implementaciones |
| **Service Layer** | `*/Application/CommandServices/` | Controladores delgados, lógica en servicios |
| **CQRS** | Command vs Query Services | Separar escritura de lectura |
| **Repository** | `IBaseRepository<T>` | Abstraer acceso a datos |
| **Unit of Work** | `IUnitOfWork` | Transacciones atómicas |
| **Chain of Resp.** | `RequestAuthorizationMiddleware` | Pipeline de middlewares |
| **Strategy** | `ITokenService`, `IHashingService` | Algoritmos intercambiables |
| **Adapter** | `*Assembler.cs` | Aislar DTOs del dominio |
| **Decorator** | `GlobalExceptionHandlerMiddleware` | Manejo global de errores |
| **Anti-Corruption** | `*ContextFacade.cs` | Aislar contextos entre sí |

---

## ¿Por qué es mejor que el monolito?

| Dimensión | Monolito | Microservicios |
|-----------|----------|---------------|
| **Disponibilidad** | Si un módulo falla, falla todo | Si Subscriptions falla, Projects sigue vivo |
| **Escalabilidad** | Escala todo o nada | Escalás solo Devices si hay mucha telemetría |
| **Despliegue** | 1 deploy (riesgo alto) | 6 deploys independientes |
| **Base de datos** | 1 BD (cuello de botella) | 5 BD separadas (sin contención) |
| **Testing** | ❌ No existía | ✅ BDD con SpecFlow (12 escenarios) |
| **Seguridad** | ❌ Sin JWT consistente | ✅ JWT + BCrypt + Blacklist |
| **Health Checks** | ❌ No existía | ✅ GET /health en todos |
| **Gateway** | ❌ Frontend → directo a API | ✅ YARP enruta y monitorea |

---

## 🧪 ¿Cómo sabemos que funciona? (Lo probamos)

```
PRUEBA 1: Sign Up     → ✅ "User created successfully."
PRUEBA 2: Sign In     → ✅ JWT con 3 claims (sid, email, role)
PRUEBA 3: Proyectos   → ✅ Proyecto creado vía Gateway
PRUEBA 4: Logout      → ✅ "Token revoked."
PRUEBA 5: Revocado    → ✅ 401 "Token has been revoked."
PRUEBA 6: Sin token   → ✅ 401 "Authorization required."
PRUEBA 7: Health      → ✅ 5/5 servicios Healthy
PRUEBA 8: Analytics   → ✅ Dashboard devuelto
PRUEBA 9: Devices     → ✅ Lista vacía (correcto)
PRUEBA 10: Planes     → ✅ Lista vacía (correcto)
```

12/12 pruebas exitosas en demo en vivo.

---

## Estructura del Repositorio

```
fundamentos_arq/
│
├── README.md                     ← Este archivo (la historia)
├── ARCHITECTURE.md                ← Estado actual as-built del sistema (ver esto para "cómo es hoy")
├── .gitignore                    ← Archivos excluidos del repo
│
├── 5.1_Section.md                ← Documentación académica del Capítulo V
├── context.md                    ← Contexto histórico del proyecto
├── deploy-flow.md                ← Flujo de despliegue actual (Azure + Terraform + Cloudflare)
│
├── ADD_Iteration_1_IoBuild.md    ← ADD Iteración 1 (documento de diseño, histórico)
├── ADD_Iteration_2_IoBuild.md    ← ADD Iteración 2 (documento de diseño, histórico)
├── ADD_Iteration_3_IoBuild.md    ← ADD Iteración 3 (documento de diseño, histórico)
├── ADD_Iteration_4_IoBuild.md    ← ADD Iteración 4 (Observabilidad y Trazabilidad Distribuida)
├── ADD_Iteration_5_IoBuild.md    ← ADD Iteración 5 (Comunicación Asíncrona entre Bounded Contexts)
│
├── local/
│   └── DEMO-JOURNEY.md           ← Guion de demo end-to-end del sistema actual
│
├── infra/                        ← Terraform (Azure VM efímera)
│   └── README.md
│
├── IoBuild-Backend/              ← Monolito ORIGINAL (no tocar)
│   └── IoBuilt.API/
│       ├── IAM/
│       ├── Projects/
│       ├── Devices/
│       ├── Subscriptions/
│       └── Shared/
│
├── IoBuild-Frontend/             ← Frontend Vue ORIGINAL
│
└── microservices/                ← ARQUITECTURA ACTUAL EN PRODUCCIÓN
    ├── README.md                 ← Documentación técnica
    ├── docker-compose.yml        ← Stack completo (dev): 7 servicios + MySQL×6 + RabbitMQ + Redis + Jaeger + Mosquitto + InfluxDB
    ├── docker-compose.prod.yml   ← Stack de producción (imágenes GHCR)
    ├── start_all.sh / kill_all.sh← Ciclo de vida de servicios
    ├── docs/
    │   ├── architecture/         ← Vista arquitectónica actual (overview.md, api-gateway-routes.md)
    │   ├── iterations/           ← Reportes por iteración ADD
    │   ├── migrations/           ← Cambios de soporte fuera de las iteraciones ADD
    │   ├── testing/              ← Evidencia de testing
    │   └── deployment/           ← Deploy actual (Azure + Terraform)
    ├── src/
    │   ├── IoBuild.Shared/       ← Librería compartida
    │   ├── IoBuild.IAM/          ← Auth (5001)
    │   ├── IoBuild.Devices/      ← IoT + telemetría MQTT→InfluxDB (5002)
    │   ├── IoBuild.Projects/     ← Proyectos (5003)
    │   ├── IoBuild.Subscriptions/← Pagos + Outbox + Idempotency (5004)
    │   ├── IoBuild.Analytics/    ← Dashboards (5005)
    │   ├── IoBuild.Profiles/     ← Perfiles de usuario (5006)
    │   └── IoBuild.Gateway/      ← Gateway YARP (8080)
    ├── tests/
    │   ├── IoBuild.IAM.Tests/    ← BDD Authentication
    │   ├── IoBuild.Devices.Tests/← BDD DeviceManagement + Telemetry
    │   ├── IoBuild.Projects.Tests/← BDD ProjectsManagement
    │   └── IoBuild.Subscriptions.Tests/← BDD SubscriptionRenewal + Outbox
    ├── mosquitto/                ← Config del broker MQTT
    ├── mysql/                    ← Scripts de inicialización (1 por servicio)
    └── iot-simulator/            ← Simulador IoT en Python
```

---

## Cómo Correr (En 30 Segundos)

```bash
# Requisitos: Docker + Docker Compose

cd microservices
docker compose up --build

# Verificar que todo esté healthy
curl http://localhost:8080/health

# Detener todo
docker compose down
```

Para desarrollo sin Docker (dotnet run por servicio) y detalles de cada base de datos, ver [`microservices/README.md`](microservices/README.md).

---

## 📚 Lecturas Recomendadas

| Documento | Contenido |
|-----------|-----------|
| [`ARCHITECTURE.md`](ARCHITECTURE.md) | **Estado actual as-built** — cómo es el sistema hoy, validado contra el código |
| [`microservices/docs/architecture/overview.md`](microservices/docs/architecture/overview.md) | Visión arquitectónica técnica detallada |
| [`local/DEMO-JOURNEY.md`](local/DEMO-JOURNEY.md) | Guion de demo end-to-end del sistema completo |
| [`deploy-flow.md`](deploy-flow.md) | Flujo de despliegue actual (Azure + Terraform + Cloudflare) |
| [`5.1_Section.md`](5.1_Section.md) | Documentación académica Capítulo V |

---

## 🧠 Filosofía del Proyecto

> *"La arquitectura no es sobre el código que escribes hoy. Es sobre el código que podrás escribir mañana sin miedo a romper lo de ayer."*

Este repositorio no es solo un proyecto universitario. Es la demostración de que:

1. **ADD funciona** — Las decisiones arquitectónicas no son al azar, son guiadas por drivers medibles
2. **Microservicios no es magia** — Es disciplina: patrones, capas, interfaces, y cada servicio con su propia BD
3. **La seguridad se diseña desde el día 1** — JWT, BCrypt, blacklist, middleware, no son agregados posteriores
4. **El testing se planea desde el principio** — SpecFlow, Gherkin, no son "si hay tiempo"
5. **Gateway no es opcional** — Sin Gateway no hay microservicios, solo monolitos partidos

---

**Ingeniería de Software — Universidad Peruana de Ciencias Aplicadas (UPC)**  
**Ciclo 2026-01 | Profesor: [Nombre del Profesor]**
# ioBuild-from-monolith-to-microservices
# ioBuild-microservices
