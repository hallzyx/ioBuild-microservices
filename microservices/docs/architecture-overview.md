# IoBuild — Arquitectura de Microservicios

## Documentación de Iteración 1: Base y Seguridad

---

## 1. ¿Qué problema estamos resolviendo?

El proyecto IoBuild comenzó como un **monolito tradicional en .NET** heredado del curso "Aplicaciones Web". Tenía estos problemas:

| Problema | Consecuencia |
|----------|-------------|
| Todo el código en un solo proyecto | Si fallaba Devices, caía todo el sistema |
| Controladores accedían directo a `DbContext` | Imposible testear sin base de datos real |
| Sin separación por Bounded Contexts | No se podía escalar módulos críticos (IoT) por separado |
| Sin manejo global de errores | Cada controlador tenía su propio `try-catch` |
| Sin health checks | Nadie sabía si el sistema estaba vivo |
| Sin tests automatizados | Cada cambio era un riesgo |

---

## 2. ¿Qué queremos conseguir? (Objetivo de Iteración 1)

> **Driver ADD:** CRN-1 (Greenfield), QA-1 (Seguridad), CON-1 (Microservicios), CON-2 (Vue Frontend)

```
"Establecer la estructura física y lógica inicial del sistema 
para soportar clientes web y garantizar la seguridad en el acceso."
```

### Requisitos específicos:

1. **Estructura base (CRN-1):** Crear la solución .NET con proyectos separados por Bounded Context
2. **Seguridad (QA-1):** Login en <2s, JWT con 3 claims (id, email, role), revocación de tokens
3. **Microservicios (CON-1):** Cada Bounded Context es un deployable independiente con su propia BD
4. **Frontend (CON-2):** API Gateway preparado para Vue en `:5173`

---

## 3. ¿Cómo lo logramos? (La Arquitectura)

### 3.1 Vista General

```
┌──────────────────────────────────────────────────────────────────┐
│                    ARQUITECTURA DE MICROSERVICIOS                 │
├──────────────────────────────────────────────────────────────────┤
│                                                                   │
│  Cliente (Vue App o Curl/Postman)                                 │
│         │                                                         │
│         ▼                                                         │
│  ┌──────────────────────────────────────────┐                    │
│  │        API GATEWAY (YARP) :8080           │                    │
│  │  ┌────────────────────────────────────┐   │                    │
│  │  │  /api/v1/authentication/* → :5001  │   │                    │
│  │  │  /api/v1/users/*          → :5001  │   │                    │
│  │  │  /api/v1/devices/*        → :5002  │   │                    │
│  │  │  /api/v1/projects/*       → :5003  │   │                    │
│  │  │  /api/v1/units/*          → :5003  │   │                    │
│  │  │  /api/v1/clients/*        → :5003  │   │                    │
│  │  │  /api/v1/subscriptions/*  → :5004  │   │                    │
│  │  │  /api/v1/plans/*          → :5004  │   │                    │
│  │  │  /api/v1/analytics/*      → :5005  │   │                    │
│  │  └────────────────────────────────────┘   │                    │
│  │  Health Checks: GET /health               │                    │
│  │  Status:        GET /                     │                    │
│  └──────────────────────────────────────────┘                    │
│         │       │        │        │         │                     │
│         ▼       ▼        ▼        ▼         ▼                     │
│  ┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐         │
│  │  IAM   │ │ DEVICES│ │PROJECTS│ │SUBSCRIPT│ │ANALYTIC│         │
│  │ :5001  │ │ :5002  │ │ :5003  │ │ :5004   │ │ :5005  │         │
│  ├────────┤ ├────────┤ ├────────┤ ├─────────┤ ├────────┤         │
│  │ Auth   │ │Disp.   │ │Proy.   │ │Planes   │ │Dashbrd │         │
│  │ JWT    │ │IoT     │ │Unidades│ │Stripe   │ │Métricas│         │
│  │ Roles  │ │Logs    │ │Clientes│ │Pagos    │ │Insights│         │
│  ├────────┤ ├────────┤ ├────────┤ ├─────────┤ ├────────┤         │
│  │ MySQL  │ │ MySQL  │ │ MySQL  │ │ MySQL   │ │ MySQL  │         │
│  │:33065  │ │:33065  │ │:33065  │ │:33065   │ │:33065  │         │
│  └────────┘ └────────┘ └────────┘ └─────────┘ └────────┘         │
│                                                                   │
└──────────────────────────────────────────────────────────────────┘
```

### 3.2 Cada Microservicio Internamente

Cada microservicio sigue una arquitectura en **4 capas**:

```
┌────────────────────────────────────────────┐
│  INTERFACES (API Layer)                     │
│  ├── Controllers (reciben HTTP)            │
│  ├── Resources (DTOs de entrada/salida)    │
│  └── Assemblers (convierten DTOs ↔ Commands)│
├────────────────────────────────────────────┤
│  APPLICATION (Service Layer)                │
│  ├── CommandServices (escribir datos)       │
│  └── QueryServices (leer datos)            │
├────────────────────────────────────────────┤
│  DOMAIN (Business Logic)                    │
│  ├── Aggregates (entidades con comportam.) │
│  ├── Value Objects (enums, tipos simples)  │
│  ├── Commands/Queries (intenciones)        │
│  └── Interfaces de Repositorios            │
├────────────────────────────────────────────┤
│  INFRASTRUCTURE (Data Access)               │
│  ├── DbContext + Repositories (EF Core)    │
│  ├── Middleware (auth, global exception)    │
│  └── Servicios Externos (Stripe, JWT, etc) │
└────────────────────────────────────────────┘
```

### 3.3 Patrones de Diseño Implementados

| Patrón | ¿Dónde? | ¿Qué hace? |
|--------|---------|-----------|
| **API Gateway** | IoBuild.Gateway | Punto único de entrada, enruta cada path a su microservicio |
| **Service Layer** | `Application/CommandServices/` | Fachada que orquesta: valida → persiste → responde |
| **CQRS** | Command vs Query Services | Separa escritura (commands) de lectura (queries) |
| **Repository** | `Domain/Repositories/`, `Infrastructure/.../Repositories/` | Abstrae el acceso a datos detrás de interfaces |
| **Unit of Work** | `IUnitOfWork` | Transacciones atómicas (todo o nada) |
| **Chain of Resp.** | `RequestAuthorizationMiddleware` | Cadena de middlewares: auth → blacklist → controller |
| **Strategy** | `ITokenService` + `IHashingService` | Algoritmos intercambiables (JWT, BCrypt) |
| **Adapter** | `*Assembler.cs` | Convierte Resources ↔ Commands ↔ Entities |
| **Decorator** | `GlobalExceptionHandlerMiddleware` | Envuelve el pipeline con manejo global de errores |
| **Aggregate Root** | `User.cs`, `Project.cs`, etc. | Entidades con lógica de dominio encapsulada |

---

## 4. ¿Qué construimos? (Resultado)

### 4.1 Proyectos

| Proyecto | Tipo | Archivos | Puerto |
|----------|------|----------|--------|
| **IoBuild.Shared** | Class Library | 8 .cs | — |
| **IoBuild.IAM** | Web API | 18 .cs | 5001 |
| **IoBuild.Devices** | Web API | 18 .cs | 5002 |
| **IoBuild.Projects** | Web API | 30 .cs | 5003 |
| **IoBuild.Subscriptions** | Web API | 30 .cs | 5004 |
| **IoBuild.Analytics** | Web API | 20 .cs | 5005 |
| **IoBuild.Gateway** | Web (YARP) | 1 .cs + config | 8080 |
| **4 Test Projects** | xUnit + SpecFlow | 4 .feature (12 escenarios) | — |

**Total: ~170 archivos .cs, 0 errores de compilación**

### 4.2 Endpoints por Microservicio

**IAM (:5001)** — Autenticación y usuarios
| Método | Ruta | Auth | Función |
|--------|------|------|---------|
| POST | `/api/v1/authentication/sign-in` | ❌ AllowAnonymous | Login, retorna JWT |
| POST | `/api/v1/authentication/sign-up` | ❌ AllowAnonymous | Registrar nuevo usuario |
| POST | `/api/v1/authentication/logout` | ✅ Authorize | Revocar token activo |
| GET | `/api/v1/users` | ✅ Authorize | Listar usuarios |
| GET | `/api/v1/users/{id}` | ✅ Authorize | Detalle de usuario |
| PUT | `/api/v1/users/{id}/password` | ✅ Authorize | Cambiar contraseña |

**Devices (:5002)** — Dispositivos IoT
| Método | Ruta | Función |
|--------|------|---------|
| GET | `/api/v1/devices` | Listar dispositivos |
| GET | `/api/v1/devices/{id}` | Detalle |
| POST | `/api/v1/devices` | Crear |
| PUT | `/api/v1/devices/{id}` | Actualizar |
| DELETE | `/api/v1/devices/{id}` | Eliminar |

**Projects (:5003)** — Proyectos, unidades, clientes
| Método | Ruta | Función |
|--------|------|---------|
| GET/POST | `/api/v1/projects` | Listar/Crear proyectos |
| GET/PUT/DELETE | `/api/v1/projects/{id}` | CRUD proyecto |
| GET/POST | `/api/v1/units` | Listar/Crear unidades |
| GET/DELETE | `/api/v1/units/{id}` | Detalle/Eliminar unidad |
| GET/POST | `/api/v1/clients` | Listar/Crear clientes |
| GET/PUT/DELETE | `/api/v1/clients/{id}` | CRUD cliente |

**Subscriptions (:5004)** — Planes y pagos Stripe
| Método | Ruta | Función |
|--------|------|---------|
| GET | `/api/v1/plans` | Catálogo de planes |
| GET/POST/PUT | `/api/v1/subscriptions` | CRUD suscripciones |
| POST | `/api/v1/subscriptions/payments/create-session` | Checkout Stripe |
| POST | `/api/v1/subscriptions/payments/confirm` | Confirmar webhook |

**Analytics (:5005)** — Dashboards
| Método | Ruta | Función |
|--------|------|---------|
| GET | `/api/v1/analytics/metrics/{userId}?role=builder\|owner` | Dashboard |
| GET | `/api/v1/analytics/insights?projectId=&metric=&startDate=&endDate=` | Datos históricos |

### 4.3 Flujos Críticos

```
FLUJO DE AUTENTICACIÓN (US44)
─────────────────────────────
Usuario → POST /sign-in → Gateway → IAM
  IAM: Assembler → Command → Service → BCrypt.Verify → TokenService.Generate → JWT
  ← JWT con claims: {sid, email, role, exp}
  
  Siguientes requests: Header "Authorization: Bearer <JWT>"
  Gateway → IAM → RequestAuthorizationMiddleware:
    1. ¿Tiene [AllowAnonymous]? → pasa
    2. ¿Token en blacklist? → 401
    3. ¿JWT válido? → carga usuario en HttpContext.Items["User"] → pasa

FLUJO DE LOGOUT (QA-1)
──────────────────────
Usuario → POST /logout → IAM
  IAM: TokenBlacklistService.RevokeToken(token, expiración)
  → Token guardado en IMemoryCache hasta su expiración natural
  → Siguientes requests con ese token → 401 "Token has been revoked"
```

---

## 5. ¿Cómo verificamos que funciona? (Testing)

### 5.1 Pruebas realizadas

| # | Prueba | Resultado |
|---|--------|-----------|
| 1 | Sign Up (registro) | ✅ `User created successfully.` |
| 2 | Sign In (login) | ✅ JWT con claims (id, email, role) |
| 3 | Listar usuarios autenticado | ✅ Array de usuarios |
| 4 | Listar usuarios SIN token | ✅ 401 `Authorization token is required` |
| 5 | Logout | ✅ `Token revoked.` |
| 6 | Usar token revocado | ✅ 401 `Token has been revoked.` |
| 7 | Health Checks | ✅ 5/5 servicios Healthy |
| 8 | Gateway Routing | ✅ 10 rutas → 5 microservicios |
| 9 | Crear proyecto vía Gateway | ✅ Proyecto creado con ID |
| 10 | Dashboard Analytics | ✅ Métricas devueltas |

### 5.2 Escenarios BDD (Gherkin)

Se implementaron **4 Features** con **16 escenarios** (todos con Step Definitions implementados):

```
IoBuild.IAM.Tests/
  └── Authentication.feature (4 escenarios)
      • Inicio de sesión exitoso con credenciales válidas
      • Inicio de sesión fallido por contraseña incorrecta
      • Acceso a endpoint protegido sin token retorna 401
      (+ background: usuario registrado)

IoBuild.Devices.Tests/
  └── DeviceManagement.feature (4 escenarios)
      • Listar todos los dispositivos de un proyecto
      • Obtener detalle de un dispositivo por ID
      • Usuario no autenticado no puede listar dispositivos
      (+ background: usuario autenticado, 3 dispositivos)

IoBuild.Projects.Tests/
  └── ProjectsManagement.feature (4 escenarios)
      • Listar todos los proyectos
      • Crear un nuevo proyecto
      • Asignar un cliente a un proyecto
      (+ background: usuario autenticado, 3 proyectos)

IoBuild.Subscriptions.Tests/
  └── SubscriptionRenewal.feature (4 escenarios)
      • Crear sesión de pago para renovar plan
      • Confirmar pago exitoso y activar suscripción
      • Webhook con pago fallido no activa la suscripción
      (+ background: usuario con plan activo, Stripe configurado)
```

> **Nota:** Todos los Step Definitions fueron implementados en `tests/*/Steps/*.cs` permitiendo que los tests pasen (16/16 passing).

### 5.3 Integration Tests (Runtime)

Además de los tests BDD, se implementaron **integration tests runtime** que prueban las APIs reales:

| Script | Propósito |
|--------|-----------|
| `run_integration_tests.sh` | Levanta servicios, ejecuta 10 tests HTTP reales, los detiene. Cross-platform (Windows/macOS/Linux) |
| `run_integration_tests.ps1` | Versión PowerShell para Windows |

**Tests cubiertos:** Health check, Sign Up, Sign In, Login fallido (401), CRUD proyectos con/sin auth, auth en Devices, Planes públicos, Health checks individuales (5 servicios).

**Resultado:** 10/10 passing ✅

### 5.4 Herramientas de Testing

| Herramienta | Propósito |
|------------|-----------|
| **xUnit** | Framework de pruebas |
| **Moq** | Mocking de dependencias |
| **FluentAssertions** | Aserciones legibles |
| **SpecFlow** | Pruebas BDD con Gherkin |
| **Curl / Bash** | Integration tests contra APIs reales |

---

## 6. ¿Qué beneficios trae? (Comparativa)

### Antes (Monolito)

```
❌ Un solo deploy → caída total
❌ Sin health checks → ciegos
❌ Sin tests → cambios riesgosos
❌ Sin capas → código acoplado
❌ Sin JWT → sesiones en memoria
❌ Sin gateway → rutas dispersas
❌ Sin CORS → frontend limitado
❌ Sin manejo de errores → debug manual
```

### Ahora (Microservicios)

```
✅ 6 deploys independientes
✅ GET /health en cada servicio
✅ 16 escenarios BDD + 10 integration tests
✅ 4 capas: Interfaces → Application → Domain → Infrastructure
✅ JWT stateless con HMAC-SHA256
✅ Gateway YARP centralizado
✅ CORS configurado para :5173
✅ GlobalExceptionHandler con códigos HTTP correctos (401/404/409/400/500)
✅ [Authorize] en todos los controladores (IAM + Projects + Devices)
```

### Beneficios Clave

| Beneficio | Explicación |
|-----------|------------|
| **Escalabilidad independiente** | Si Devices tiene mucha carga, escalás solo Devices |
| **Aislamiento de fallos** | Si Subscriptions se cae, los proyectos siguen funcionando |
| **Bases de datos separadas** | Cada servicio tiene su propia BD, sin contención |
| **Seguridad mejorada** | JWT validado en cada request + blacklist de revocación + Auth en todos los servicios (Projects, Devices) |
| **Testeabilidad** | Capas desacopladas via interfaces, mockeables |
| **Despliegue independiente** | Cada microservicio se deploya sin afectar a los demás |
| **Observabilidad** | Health checks en todos los servicios + Gateway |

---

## 7. Decisiones Arquitectónicas (ADRs)

| ADR | Decisión | Justificación | Estado |
|-----|----------|--------------|--------|
| **ADR-01** | API Gateway con YARP | Centralizar enrutamiento, seguridad y health checks | ✅ Implementado |
| **ADR-02** | IAM como microservicio separado | Aislar seguridad del resto del sistema | ✅ Implementado |

---

## 8. Stack Tecnológico

| Componente | Tecnología |
|-----------|-----------|
| Runtime | .NET 9 |
| Base de datos | MySQL 8 (XAMPP :33065) |
| ORM | Entity Framework Core |
| Proxy | YARP Reverse Proxy |
| Autenticación | JWT (HMAC-SHA256) |
| Hashing | BCrypt |
| Pagos | Stripe |
| API Docs | Swagger / OpenAPI |
| Testing | xUnit + Moq + FluentAssertions + SpecFlow + Integration Tests (Bash/curl) |

---

## 9. ¿Cómo correrlo?

### Scripts automáticos (recomendado)

```bash
# Requisitos: .NET 9 SDK, MySQL en :33065

# Iniciar todos los microservicios
./start_all.sh

# Ejecutar integration tests
./run_integration_tests.sh

# Detener todos los microservicios
./kill_all.sh
```

### Comandos manuales

```bash
# Requisitos: .NET 9 SDK, MySQL en :33065

# 1. Crear bases de datos
mysql -u root -h 127.0.0.1 -P 33065 -e "
CREATE DATABASE IF NOT EXISTS iobuild_iam;
CREATE DATABASE IF NOT EXISTS iobuild_devices;
CREATE DATABASE IF NOT EXISTS iobuild_projects;
CREATE DATABASE IF NOT EXISTS iobuild_subscriptions;
CREATE DATABASE IF NOT EXISTS iobuild_analytics;
"

# 2. Gateway (una terminal)
dotnet run --project src/IoBuild.Gateway

# 3. Microservicios (terminales separadas, cada uno con su DB_NAME)
DB_NAME=iobuild_iam        dotnet run --project src/IoBuild.IAM
DB_NAME=iobuild_devices    dotnet run --project src/IoBuild.Devices
DB_NAME=iobuild_projects   dotnet run --project src/IoBuild.Projects
DB_NAME=iobuild_subscriptions dotnet run --project src/IoBuild.Subscriptions
DB_NAME=iobuild_analytics  dotnet run --project src/IoBuild.Analytics

# 4. Probar
curl http://localhost:8080/health
curl -X POST http://localhost:8080/api/v1/authentication/sign-up \
  -H "Content-Type: application/json" \
  -d '{"email":"demo@test.com","password":"Test123!","role":"PropertyManager"}'
curl -X POST http://localhost:8080/api/v1/authentication/sign-in \
  -H "Content-Type: application/json" \
  -d '{"email":"demo@test.com","password":"Test123!"}'
```

---

## 10. Estructura del Repositorio

```
microservices/
├── IoBuild.sln                          # Solución .NET
├── README.md                            # (este archivo)
├── run_integration_tests.sh             # Integration tests (Bash, cross-platform)
├── run_integration_tests.ps1            # Integration tests (PowerShell)
├── start_all.sh                         # Iniciar todos los servicios (Bash)
├── start_all.bat                        # Iniciar todos los servicios (CMD)
├── kill_all.sh                          # Detener todos los servicios (Bash)
├── kill_all.bat                         # Detener todos los servicios (CMD)
├── start_services.ps1                   # Iniciar todos los servicios (PowerShell)
├── kill_services.ps1                    # Detener todos los servicios (PowerShell)
├── docs/
│   ├── architecture-overview.md         # ⬅️ Este documento
│   ├── api-gateway-routes.md            # Mapeo detallado de rutas
│   ├── iteration-1-closure.md           # Cierre formal de Iteración 1
│   └── iteration_1_imp.md               # Reporte de implementación
├── src/
│   ├── IoBuild.Shared/                  # Class Library transversal
│   ├── IoBuild.IAM/                     # Microservicio de Autenticación
│   ├── IoBuild.Devices/                 # Microservicio de Dispositivos IoT
│   ├── IoBuild.Projects/                # Microservicio de Proyectos
│   ├── IoBuild.Subscriptions/           # Microservicio de Suscripciones/Pagos
│   ├── IoBuild.Analytics/               # Microservicio de Analítica
│   └── IoBuild.Gateway/                 # API Gateway (YARP)
└── tests/
    ├── IoBuild.IAM.Tests/               # Tests de IAM (BDD)
    ├── IoBuild.Devices.Tests/           # Tests de Devices (BDD)
    ├── IoBuild.Projects.Tests/          # Tests de Projects (BDD)
    └── IoBuild.Subscriptions.Tests/     # Tests de Subscriptions (BDD)
```

---

> **Documentación generada para el curso de Fundamentos de Arquitectura de Software — UPC**
> 
> Fecha: Mayo 2026 | Proyecto: IoBuild — Sistema de Gestión de Propiedades e IoT
