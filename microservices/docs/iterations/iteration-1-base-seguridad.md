# IoBuild — Iteration 1 Final Report

## Del Monolito a Producción: La Historia Completa

**Proyecto:** IoBuild — Sistema de Gestión de Propiedades e IoT  
**Curso:** Fundamentos de Arquitectura de Software — UPC  
**Fecha:** Mayo 2026  
**Estado Final:** Desplegado en Producción ✅

---

## Índice

1. [El Punto de Partida: El Monolito Heredado](#1-el-punto-de-partida-el-monolito-heredado)
2. [La Visión: Qué Queríamos Construir](#2-la-visión-qué-queríamos-construir)
3. [Fase 1: Deconstrucción del Monolito](#3-fase-1-deconstrucción-del-monolito)
4. [Fase 2: Testing, Seguridad y Calidad](#4-fase-2-testing-seguridad-y-calidad)
5. [Fase 3: Dockerización Completa](#5-fase-3-dockerización-completa)
6. [Fase 4: Despliegue a Producción (VPS + Dokploy)](#6-fase-4-despliegue-a-producción-vps--dokploy)
7. [Decisiones Arquitectónicas (ADRs)](#7-decisiones-arquitectónicas-adrs)
    - [7.1 Defensa: Traefik + Nginx + YARP](#71-defensa-arquitectónica-traefik--nginx--yarp)
9. [La "Magia" de Transformar un Monolito](#9-la-magia-de-transformar-un-monolito)
10. [Línea de Tiempo de Commits](#10-línea-de-tiempo-de-commits)
11. [Estadísticas Finales](#11-estadísticas-finales)
12. [Arquitectura Final de Producción](#12-arquitectura-final-de-producción)
13. [Lecciones Aprendidas](#13-lecciones-aprendidas)

---

## 1. El Punto de Partida: El Monolito Heredado

### Origen

El proyecto IoBuild se heredó del curso anterior **"Aplicaciones Web"**, estructurado como un **monolito tradicional en .NET**. Era una Web API única que contenía absolutamente todo.

### Problemas Arquitectónicos del Monolito

| Problema | Consecuencia Real |
|----------|------------------|
| **Todo en un solo proyecto** | Si fallaba Devices, caía Users. Si fallaba Users, caía Projects. Sin aislamiento de fallos. |
| **Controladores acoplados a `DbContext`** | `ProjectsController` instanciaba `DbContext` directamente. Imposible testear sin base de datos real. |
| **Sin Bounded Contexts** | No había separación conceptual entre módulos. Manejo de usuarios, registro de dispositivos IoT, control de proyectos — todo revuelto en un mismo namespace. |
| **Sin manejo global de errores** | Cada controlador tenía su propio `try-catch`. Si un error no se capturaba, el cliente recibía un 500 genérico sin información útil. |
| **Sin health checks** | Nadie sabía si el sistema estaba vivo. No había monitoreo. |
| **Sin tests automatizados** | Cada cambio era un riesgo. No existía ninguna suite de pruebas. |
| **Sin separación de responsabilidades** | Lógica de negocio, acceso a datos y presentación HTTP estaban mezclados en los controladores. |

### Estado Inicial en Código

```
MonolitoAnterior/
├── MonolitoAnterior.sln
├── MonolitoAnterior.Api/
│   ├── Controllers/
│   │   ├── AuthController.cs       # Auth + Users mezclados
│   │   ├── DevicesController.cs    # Acceso directo a DbContext
│   │   ├── ProjectsController.cs   # try-catch manual en cada método
│   │   └── SubscriptionsController.cs
│   ├── Models/                     # Entidades planas, sin lógica de dominio
│   ├── Data/
│   │   └── AppDbContext.cs         # Un solo DbContext para todo
│   └── Program.cs                  # Configuración mínima
└── (sin tests, sin CI/CD, sin Docker)
```

---

## 2. La Visión: Qué Queríamos Construir

### Drivers Arquitectónicos (ADD v3)

| Driver | Tipo | Descripción |
|--------|------|------------|
| **CRN-1** | Concern (Greenfield) | Establecer la estructura física y lógica inicial del sistema para soportar clientes web |
| **QA-1** | Quality Attribute (Seguridad) | Login en <2s, JWT con 3 claims (id, email, role), revocación de tokens activos |
| **CON-1** | Constraint (Microservicios) | Cada Bounded Context es un deployable independiente con su propia base de datos |
| **CON-2** | Constraint (Vue Frontend) | La arquitectura debe prepararse para un cliente Vue.js que consuma las APIs |

### Objetivo de la Iteración 1

> *"Establecer la estructura física y lógica inicial del sistema para soportar clientes web y garantizar la seguridad en el acceso."*

### Metas Concretas

1. **Deconstruir** el monolito en microservicios por Bounded Context
2. **Extraer** una librería compartida (`IoBuild.Shared`) con código transversal
3. **Implementar** API Gateway como punto único de entrada
4. **Aplicar** 10+ patrones GoF y principios SOLID en la nueva estructura
5. **Crear** suite de testing BDD + Integration
6. **Dockerizar** todos los servicios para despliegue consistente
7. **Desplegar** en producción sobre VPS con Dokploy

---

## 3. Fase 1: Deconstrucción del Monolito

### 3.1 Identificación de Bounded Contexts

El primer paso fue analizar el dominio de IoBuild y trazar líneas claras entre responsabilidades:

```
┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐
│     IAM      │  │   DEVICES    │  │   PROJECTS   │  │ SUBSCRIPTIONS│  │  ANALYTICS   │
│  Identidad   │  │  Dispositivos│  │  Proyectos   │  │    Pagos     │  │  Dashboards  │
│  + Acceso    │  │     IoT      │  │  + Clientes  │  │  + Planes    │  │  + Métricas  │
└──────────────┘  └──────────────┘  └──────────────┘  └──────────────┘  └──────────────┘
       │                │                │                │                │
       ▼                ▼                ▼                ▼                ▼
  ┌─────────┐    ┌─────────┐    ┌─────────┐    ┌─────────┐    ┌─────────┐
  │MySQL IAM│    │MySQL Dev│    │MySQL Pro│    │MySQL Sub│    │MySQL Ana│
  └─────────┘    └─────────┘    └─────────┘    └─────────┘    └─────────┘
```

**¿Por qué 5 microservicios y no 3 o 7?**

Porque cada uno representa un **Bounded Context** bien definido del dominio de construcción IoT:

- **IAM** maneja identidad, autenticación y autorización — separarlo es crítico porque la seguridad no debe mezclarse con lógica de negocio.
- **Devices** gestiona hardware IoT — va a escalar distinto (mucha telemetría, pocas operaciones CRUD).
- **Projects** maneja la lógica de negocio principal — proyectos de construcción, unidades, clientes.
- **Subscriptions** maneja pagos Stripe — si Stripe falla, no debe tumbar el resto del sistema.
- **Analytics** maneja dashboards y métricas — consultas pesadas que no deben competir con operaciones transaccionales.

### 3.2 Extracción de la Librería Compartida: `IoBuild.Shared`

**Decisión crítica:** En vez de duplicar código en 5 microservicios, centralizamos todo lo transversal en una Class Library.

```
IoBuild.Shared/
├── Domain/
│   ├── Repositories/
│   │   ├── IBaseRepository.cs          # Contrato CRUD genérico
│   │   └── IUnitOfWork.cs              # Transacciones atómicas
│   └── Model/
│       └── Events/
│           └── IEvent.cs               # Eventos de dominio
├── Infrastructure/
│   ├── Middleware/
│   │   ├── GlobalExceptionHandlerMiddleware.cs   # Decorator: manejo global de errores
│   │   └── JwtAuthenticationMiddleware.cs        # Auth compartida para Projects, Devices
│   ├── Tokens/
│   │   ├── TokenSettings.cs            # Configuración JWT unificada
│   │   └── ITokenBlacklistService.cs   # Revocación de tokens (QA-1)
│   ├── ASP/
│   │   └── Configuration/
│   │       ├── AuthorizeAttribute.cs       # Atributo [Authorize] compartido
│   │       ├── AllowAnonymousAttribute.cs   # Atributo [AllowAnonymous] compartido
│   │       ├── KebabCaseRouteNamingConvention.cs  # Convención de rutas
│   │       └── Extensions/
│   │           └── StringExtensions.cs     # Helper ToKebabCase()
│   └── EFC/
│       └── Extensions/
│           └── ModelBuilderExtensions.cs   # Convención snake_case para BD
```

**¿Por qué una Class Library y no paquete NuGet?**
- El equipo es uno solo, no hay consumidores externos.
- Iteración rápida: cambios en Shared se propagan instantáneamente con rebuild.
- No necesitamos versionado semántico ni pipeline de publicación.

**¿Por qué incluir middleware de auth aquí?**
- Los microservicios Projects y Devices necesitan validar JWT pero no deberían duplicar lógica de autenticación.
- Centralizar el middleware en Shared permite que cualquier microservicio futuro herede seguridad automáticamente.

### 3.3 Arquitectura Interna de Cada Microservicio

Cada microservicio implementa **Clean Architecture en 4 capas**:

```
┌─────────────────────────────────────────────────────────┐
│                    INTERFACES (API Layer)                 │
│  Controllers REST + Resources (DTOs) + Assemblers        │
│  Responsabilidad: Recibir HTTP, devolver HTTP             │
├─────────────────────────────────────────────────────────┤
│                   APPLICATION (Service Layer)             │
│  CommandServices (escritura) + QueryServices (lectura)    │
│  Responsabilidad: Orquestar casos de uso                  │
├─────────────────────────────────────────────────────────┤
│                      DOMAIN (Business Logic)              │
│  Aggregates + Value Objects + Commands/Queries +         │
│  Interfaces de Repositorios + Interfaces de Servicios     │
│  Responsabilidad: Reglas de negocio puras                 │
├─────────────────────────────────────────────────────────┤
│                  INFRASTRUCTURE (Data Access)             │
│  DbContext + Repositories (EF Core) + Servicios Externos │
│  Responsabilidad: Persistencia, JWT, Stripe, BCrypt      │
└─────────────────────────────────────────────────────────┘
```

**¿Por qué 4 capas? La regla de dependencia:**

```
Interfaces → Application → Domain ← Infrastructure
```

- **Domain** no depende de nadie. Es el corazón. Las reglas de negocio viven aquí.
- **Application** depende de Domain. Orquesta casos de uso sin saber de HTTP ni de bases de datos.
- **Infrastructure** implementa las interfaces que define Domain. EF Core, Stripe, BCrypt.
- **Interfaces** es la capa más externa. Traduce HTTP a comandos del Application Layer.

**Beneficio concreto:** Si mañana queremos cambiar de MySQL a PostgreSQL, solo tocamos Infrastructure. El Domain ni se entera.

### 3.4 API Gateway: YARP Reverse Proxy

```
                    ┌─────────────────────────────┐
                    │      API GATEWAY (:8080)      │
                    │         YARP Proxy            │
                    │                               │
  Cliente ────────▶│ /api/v1/authentication/* → IAM │
  (Vue/Navegador) │ /api/v1/users/*          → IAM │
                    │ /api/v1/devices/*        → Dev │
                    │ /api/v1/projects/*       → Pro │
                    │ /api/v1/subscriptions/*  → Sub │
                    │ /api/v1/analytics/*      → Ana │
                    │                               │
                    │ GET /health → Estado de todos  │
                    └─────────────────────────────┘
```

**¿Por qué YARP?**
- **YARP** es el proxy oficial de Microsoft para .NET. Integración nativa con el ecosistema.
- Configuración declarativa en `appsettings.json` — cero archivos de configuración externos.
- Health checks activos integrados con política de reintentos.
- Mismo pipeline de CI/CD que el resto de servicios .NET.

**¿Por qué un Gateway?**
- **Único punto de entrada:** El frontend solo conoce una URL base. El Gateway enruta cada path al microservicio correcto.
- **CORS centralizado:** Una sola política CORS en el Gateway, no 5 configuraciones dispersas.
- **Health checks agregados:** El Gateway monitorea la salud de todos los servicios y expone un endpoint unificado.
- **Seguridad:** Validación temprana de requests antes de llegar a los microservicios.

### 3.5 Patrones GoF y SOLID Implementados

| Patrón | Dónde | Problema que Resuelve | Tipo GoF |
|--------|-------|----------------------|----------|
| **API Gateway** | `IoBuild.Gateway` | Múltiples puntos de entrada → uno solo | Arquitectónico |
| **Service Layer (Facade)** | `Application/CommandServices/` | Controladores con lógica de negocio → orquestación delegada | Estructural |
| **CQRS** | Command vs Query Services | Mezcla lectura/escritura → segregación de responsabilidades | Arquitectónico |
| **Repository** | `Domain/Repositories/` + `Infrastructure/*/Repositories/` | Acceso directo a BD → abstracción detrás de interfaces | Estructural |
| **Unit of Work** | `IUnitOfWork` | Transacciones inconsistentes → atómicas (todo o nada) | Comportamiento |
| **Chain of Responsibility** | `RequestAuthorizationMiddleware` | Validación dispersa → pipeline de middlewares encadenados | Comportamiento |
| **Strategy** | `ITokenService` + `IHashingService` | Algoritmos fijos → intercambiables (JWT, BCrypt) | Comportamiento |
| **Adapter** | `*Assembler.cs` | DTOs ↔ Commands ↔ Entities incompatibles → conversión limpia | Estructural |
| **Decorator** | `GlobalExceptionHandlerMiddleware` | try-catch en cada método → wrapper global del pipeline | Estructural |
| **Aggregate Root** | `User.cs`, `Project.cs`, `Device.cs` | Entidades anémicas → entidades con lógica de dominio encapsulada | DDD |
| **Dependency Injection** | `Program.cs` en todos los servicios | Acoplamiento duro → contratos (Interfaces) inyectados | SOLID (DIP) |

### 3.6 Proyectos Creados

| # | Proyecto | Tipo | Puerto | Archivos .cs |
|---|----------|------|--------|-------------|
| 1 | `IoBuild.Shared` | Class Library | — | 8 |
| 2 | `IoBuild.IAM` | Web API | 5001 | 18 |
| 3 | `IoBuild.Devices` | Web API | 5002 | 18 |
| 4 | `IoBuild.Projects` | Web API | 5003 | 30 |
| 5 | `IoBuild.Subscriptions` | Web API | 5004 | 30 |
| 6 | `IoBuild.Analytics` | Web API | 5005 | 20 |
| 7 | `IoBuild.Gateway` | Web (YARP) | 8080 | 1 + config |

**Total: ~125 archivos .cs fuente, 0 errores de compilación, 0 warnings.**

---

## 4. Fase 2: Testing, Seguridad y Calidad

### 4.1 BDD con SpecFlow (16 Escenarios)

Se implementaron 4 proyectos de test con SpecFlow + xUnit + Moq + FluentAssertions:

| Proyecto de Test | Feature File | Escenarios | Step Definitions |
|-----------------|-------------|-----------|-----------------|
| `IoBuild.IAM.Tests` | `Authentication.feature` | 4 | `AuthenticationSteps.cs` |
| `IoBuild.Devices.Tests` | `DeviceManagement.feature` | 4 | `DeviceManagementSteps.cs` |
| `IoBuild.Projects.Tests` | `ProjectsManagement.feature` | 4 | `ProjectsManagementSteps.cs` |
| `IoBuild.Subscriptions.Tests` | `SubscriptionRenewal.feature` | 4 | `SubscriptionRenewalSteps.cs` |

**Resultado: 16/16 passing ✅**

**¿Por qué BDD con SpecFlow?**
- Las User Stories del curso se mapean directamente a escenarios Gherkin.
- El lenguaje Given-When-Then es entendible por stakeholders no técnicos.
- Los tests documentan el comportamiento esperado del sistema.

### 4.2 Integration Tests Runtime (10 Escenarios)

Además de los tests BDD con mocks, se crearon **integration tests contra APIs reales**:

| Script | Propósito |
|--------|-----------|
| `run_integration_tests.sh` | Levanta servicios, ejecuta 10 tests HTTP reales, los detiene. Cross-platform (Windows/macOS/Linux) |
| `run_integration_tests.ps1` | Versión PowerShell para Windows |

**Tests cubiertos:** Health check, Sign Up, Sign In, Login fallido (401), CRUD proyectos con/sin auth, auth en Devices, Planes públicos, Health checks individuales (5 servicios).

**Resultado: 10/10 passing ✅**

### 4.3 Auditoría de Seguridad y Correcciones Críticas

Durante los integration tests se descubrieron **3 vulnerabilidades graves**:

#### 🔴 Vulnerabilidad 1: Excepciones sin Distinción HTTP

**Problema:** `GlobalExceptionHandlerMiddleware` mapeaba TODAS las excepciones a 500 Internal Server Error. Un login con contraseña incorrecta retornaba 500 en vez de 401.

**Fix:** El middleware ahora distingue tipos de excepción:
```csharp
UnauthorizedAccessException  → 401 Unauthorized
ArgumentException             → 400 Bad Request
KeyNotFoundException          → 404 Not Found
InvalidOperationException     → 409 Conflict
Exception (genérica)          → 500 Internal Server Error
```

#### 🔴 Vulnerabilidad 2: Endpoints Sin Protección

**Problema:** Los controladores de Projects, Devices, Units y Clients **no tenían `[Authorize]`**. Cualquier persona sin autenticarse podía crear proyectos o listar dispositivos.

**Fix:** Se creó `JwtAuthenticationMiddleware` en `IoBuild.Shared`, se registró en los `Program.cs` de Projects y Devices, y se aplicó `[Authorize]` en todos los controladores relevantes (10 controladores).

**Impacto:** 10 endpoints pasaron de ser totalmente abiertos a requerir autenticación JWT válida.

#### 🔴 Vulnerabilidad 3: Token Settings Duplicados y Desincronizados

**Problema:** `TokenSettings` estaba definido en IAM y potencialmente diferente en otros servicios. Si la clave secreta no coincidía, el JWT era rechazado.

**Fix:** Se unificó `TokenSettings` en `IoBuild.Shared`. IAM y todos los servicios consumen la misma configuración desde un solo lugar.

---

## 5. Fase 3: Dockerización Completa

### 5.1 Dockerfiles por Microservicio

Se crearon Dockerfiles multi-stage para cada servicio .NET y el frontend:

| Dockerfile | Stage 1 (Build) | Stage 2 (Runtime) | Tamaño Imagen |
|-----------|-----------------|-------------------|--------------|
| `IoBuild.IAM/Dockerfile` | `dotnet/sdk:9.0` | `dotnet/aspnet:9.0` | ~220 MB |
| `IoBuild.Devices/Dockerfile` | `dotnet/sdk:9.0` | `dotnet/aspnet:9.0` | ~220 MB |
| `IoBuild.Projects/Dockerfile` | `dotnet/sdk:9.0` | `dotnet/aspnet:9.0` | ~220 MB |
| `IoBuild.Subscriptions/Dockerfile` | `dotnet/sdk:9.0` | `dotnet/aspnet:9.0` | ~220 MB |
| `IoBuild.Analytics/Dockerfile` | `dotnet/sdk:9.0` | `dotnet/aspnet:9.0` | ~220 MB |
| `IoBuild.Gateway/Dockerfile` | `dotnet/sdk:9.0` | `dotnet/aspnet:9.0` | ~220 MB |
| `frontend-docker/Dockerfile` | `node:20-alpine` | `nginx:alpine` | ~50 MB |

**Estrategia multi-stage:** Separar build de runtime reduce el tamaño de la imagen final en ~60%. La imagen de runtime solo contiene los binarios compilados y el runtime de .NET, sin SDK, sin código fuente, sin herramientas de build.

### 5.2 Frontend: Containerización Sin Modificar el Monolito Original

**Restricción:** No modificar el repositorio original del frontend monolito.

**Solución:** Crear una **copia independiente** bajo `microservices/frontend-docker/`:

```
frontend-docker/
├── Dockerfile                    # Build Node + Runtime Nginx
├── nginx.conf                    # Proxy reverso Nginx → Gateway
└── IoBuild-Frontend/             # Copia completa del frontend
    ├── src/
    │   └── shared/
    │       └── infrastructure/
    │           └── base-api.js   # Fix: interceptores 401, rutas públicas
    ├── .env.production           # VITE_API_URL + endpoints
    └── ...
```

**Problemas resueltos en el frontend:**

1. **`api/v1/undefined/sign-in`:** Faltaban variables de entorno Vite para los paths de endpoints (`VITE_AUTHENTICATION_ENDPOINT_PATH`, `VITE_DEVICES_ENDPOINT_PATH`, etc.). No solo la URL base.

2. **Login 401 → redirect infinito:** El interceptor de axios redirigía a `/login` cuando la ruta real es `/iam/login`. Se corrigió `base-api.js` para detectar rutas públicas (`/iam/login`, `/iam/register*`).

3. **Error de submodule git:** Al copiar el frontend, el `.git` anidado hizo que Dokploy fallara en `npm ci`. Se eliminó el metadata de git anidado y se añadieron los archivos como tracked files normales.

### 5.3 Docker Compose: Orquestación

#### `docker-compose.yml` (Producción)

```yaml
services:
  mysql:        # MySQL 8.0 — init.sql crea 5 bases de datos
  iam:          # :5001 — depende de mysql healthy
  devices:      # :5002 — depende de iam healthy
  projects:     # :5003 — depende de iam healthy
  subscriptions: # :5004 — depende de iam healthy
  analytics:    # :5005 — depende de iam healthy
  gateway:      # :8080 — depende de TODOS healthy
  frontend:     # :80 (interno) — depende de gateway healthy
```

**Decisiones de diseño:**

1. **`depends_on` con `condition: service_healthy`:** Los servicios no arrancan hasta que sus dependencias pasan health checks. Esto evita race conditions al inicio.

2. **`mem_limit` por servicio:** 256 MB para microservicios .NET, 128 MB para el Gateway, 384 MB para MySQL. Optimizado para VPS de 2 GB RAM.

3. **`expose` en vez de `ports` para producción:** Los puertos no se exponen al host. Solo Traefik (Dokploy) puede acceder. Esto reduce la superficie de ataque.

4. **Variables de entorno con fallback:** `${DB_PASSWORD:-iobuild123}` — si la variable no está definida, usa el fallback. Funciona tanto en desarrollo local como en producción con secrets.

#### `docker-compose.override.yml` (Desarrollo Local)

```yaml
# Se mergea automáticamente. Solo expone puertos para debugging local.
gateway:    ports: ["8080:8080"]
iam:        ports: ["5001:5001"]
devices:    ports: ["5002:5002"]
# ... etc
```

**¿Por qué un override?** En local necesitás acceso directo a cada servicio para debugging con Postman o Swagger. En producción, solo el Gateway (vía Nginx/Traefik) es accesible.

### 5.4 Configuración de Nginx (Frontend)

```nginx
server {
    listen 80;
    root /usr/share/nginx/html;
    index index.html;

    # API → Gateway (microservicios)
    location /api/ {
        proxy_pass http://gateway:8080;
        # ... headers para WebSocket y proxy
    }

    # SPA fallback (Vue Router)
    location / {
        try_files $uri $uri/ /index.html;
    }
}
```

**¿Por qué Nginx y no servir los archivos estáticos desde .NET?**
- Nginx es el servidor web más rápido del mundo para archivos estáticos.
- Separación de responsabilidades: Nginx sirve la UI, Gateway enruta APIs.
- Si el frontend se cae, las APIs siguen funcionando para otros clientes (mobile, etc.).

---

## 6. Fase 4: Despliegue a Producción (VPS + Dokploy)

### 6.1 Infraestructura

| Componente | Detalle |
|-----------|--------|
| **VPS** | Linux (Ubuntu), 2 GB RAM, 2 vCPU |
| **Orquestador** | Dokploy (auto-hosted, Traefik integrado) |
| **Proxy Reverso** | Traefik (manejado por Dokploy) |
| **TLS** | Let's Encrypt vía Traefik (auto-renovación) |
| **Dominio** | `io-build-v2.arroz.dev` |
| **Base de Datos** | MySQL 8.0 en contenedor Docker (sin servicio externo) |

### 6.2 Problemas Encontrados y Soluciones en Producción

#### 🔴 Problema 1: Submodule Git y `npm ci`

**Síntoma:** `npm ci` fallaba en el build del frontend con error de permisos.

**Causa:** La copia del frontend contenía su propio `.git`, haciendo que Git lo tratara como submodule. Dokploy clonaba el repositorio y los archivos del "submodule" no se descargaban.

**Solución:** `rm -rf frontend-docker/IoBuild-Frontend/.git` y volver a añadir los archivos como archivos normales tracked por Git.

#### 🔴 Problema 2: Memoria Insuficiente en VPS

**Síntoma:** Contenedores morían aleatoriamente con OOM (Out of Memory).

**Causa:** 8 contenedores (MySQL + 5 microservicios + Gateway + Frontend) sin límites de memoria en un VPS de 2 GB.

**Solución:**
- Aplicar `mem_limit` a cada servicio (128-384 MB).
- Arranque secuencial con `depends_on` + `condition: service_healthy`. Los servicios arrancan de a uno, no todos simultáneamente, reduciendo el pico de consumo de RAM.

#### 🔴 Problema 3: Puerto 80 en Conflicto con Traefik

**Síntoma:** El frontend fallaba al iniciar — "port 80 already allocated".

**Causa:** El `docker-compose.yml` base tenía `ports: 80:80` en el servicio frontend. Pero Traefik (de Dokploy) ya ocupa el puerto 80 del host.

**Solución:** Quitar `ports` del frontend en el compose base. Usar solo `expose: 80` para que Traefik enrute internamente. El `docker-compose.override.yml` (local) sí expone `80:80` porque en desarrollo local no hay Traefik.

#### 🔴 Problema 4: Health Check del Frontend con `wget`

**Síntoma:** El contenedor frontend aparecía como **unhealthy** a pesar de que Nginx estaba corriendo.

**Causa:** La imagen `nginx:alpine` usa BusyBox. Su `wget` no soporta el flag `--spider` que usaba el health check. El comando fallaba silenciosamente, Traefik veía el contenedor como no saludable, y no enrutaba tráfico. **Resultado: 404 en el dominio.**

**Solución:**
1. Agregar `RUN apk add --no-cache curl` en el Dockerfile del frontend.
2. Cambiar el health check a: `["CMD", "curl", "-f", "http://localhost:80/"]`.

**Este fue el bug final que impedía ver la app en producción.**

#### 🟡 Problema 5: Configuración de Dominio en Dokploy

**Síntoma:** URL mostraba error de conexión en vez del frontend.

**Causa:** El dominio en Dokploy apuntaba a `Container Port: 3000` (el default de Vite dev server), pero el frontend en producción usa Nginx en puerto `80`.

**Solución:** Configurar en la UI de Dokploy: Service=`frontend`, Container Port=`80`, HTTPS=ON.

### 6.3 Estado Final de Producción

```
$ docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

iobuild-frontend          Up (healthy)     80/tcp
iobuild-gateway           Up (healthy)     8080/tcp
iobuild-projects          Up (healthy)     5003/tcp
iobuild-devices           Up (healthy)     5002/tcp
iobuild-subscriptions     Up (healthy)     5004/tcp
iobuild-analytics         Up (healthy)     5005/tcp
iobuild-iam               Up (healthy)     5001/tcp
iobuild-mysql             Up (healthy)     3306/tcp
```

**8/8 contenedores healthy. App funcionando en `https://io-build-v2.arroz.dev`.**

---

## 7. Decisiones Arquitectónicas (ADRs)

| ADR | Decisión | Justificación | Trade-off Aceptado |
|-----|----------|--------------|-------------------|
| **ADR-01** | API Gateway con YARP | Centralizar enrutamiento, seguridad y health checks. Un solo punto de entrada para el frontend. | Single Point of Failure — mitigado con health checks activos y política de reintentos |
| **ADR-02** | IAM como microservicio separado | Aislar seguridad del resto del sistema. Si IAM falla, los demás servicios pueden seguir operando con tokens ya emitidos. | Latencia extra por llamada HTTP — mitigado porque el JWT es stateless (no requiere llamada a IAM en cada request) |
| **ADR-03** | MySQL compartido (misma instancia, diferentes BDs) | En Iteración 1, 5 instancias de MySQL en un VPS de 2 GB es inviable. Una instancia con 5 bases de datos separadas es el compromiso pragmático. | No hay aislamiento total de fallos a nivel BD — aceptable para MVP, migrable a instancias separadas en el futuro |
| **ADR-04** | Docker Compose (no Kubernetes) | Para 8 contenedores en un VPS pequeño, Kubernetes es overkill masivo. Docker Compose + Dokploy da orquestación suficiente con mínima complejidad operativa. | Menos escalabilidad horizontal automatizada — aceptable para la escala actual |
| **ADR-05** | Clean Architecture (4 capas) en cada microservicio | Separación de responsabilidades, testeabilidad, independencia de frameworks externos. El Domain no conoce EF Core ni ASP.NET. | Más archivos y carpetas — compensado por claridad estructural y facilidad de onboarding |
| **ADR-06** | Frontend como copia independiente (no monorepo) | No modificar el monolito original. La copia tiene sus propias variables de entorno y fixes sin afectar el repo original. | Divergencia potencial entre copia y original — mitigado documentando los fixes aplicados |
| **ADR-07** | `depends_on` con health checks en vez de `wait-for-it` | Los health checks son nativos de Docker, no requieren scripts externos, y verifican que el servicio está realmente listo (no solo el puerto abierto). | Tiempo de arranque inicial más lento — aceptable porque solo ocurre en deploy, no en runtime |
| **ADR-08** | Tres capas de proxy: Traefik + Nginx + YARP | Cada capa tiene una responsabilidad distinta y no redundante: Traefik = TLS + edge routing, Nginx = archivos estáticos + SPA fallback, YARP = API Gateway con health checks agregados. | Percepción de "sobre-ingeniería" — mitigado documentando la separación clara de responsabilidades y el hecho de que dos de los tres componentes (Traefik, Nginx) son ubicuos en la industria |

---

### 7.1 Defensa Arquitectónica: Traefik + Nginx + YARP

#### El Problema Aparente

A primera vista, tener **tres proxies** (Traefik, Nginx, YARP) encadenados parece redundante.

Es una preocupación **válida**, y esta sección existe precisamente para defender la decisión con argumentos técnicos concretos.

#### Separación de Responsabilidades

Cada capa del pipeline de proxies resuelve un problema distinto. No se pisan, se complementan:

```
Internet (HTTPS)
      │
      ▼
┌──────────────────────────────────────────────────────────────┐
│ TRAEFIK                                                       │
│ Responsabilidad: Edge Proxy — TLS + Certificados + Routing   │
│                                                                │
│ - Let's Encrypt: HTTPS automático con renovación sin downtime │
│ - Routing dinámico: Lee Docker labels, no requiere reinicios  │
│ - Aislamiento unhealthy: No enruta a contenedores caídos      │
│ - Rate limiting, IP whitelist, gzip (middlewares built-in)   │
│                                                                │
│ ¿Se puede quitar? Si pero... Sin él,      │
│ tendríamos que manejar certbot + Nginx host-level a mano.     │
└──────────────────────────┬───────────────────────────────────┘
                           │ HTTP :80 (docker network)
                           ▼
┌──────────────────────────────────────────────────────────────┐
│ NGINX (contenedor frontend)                                    │
│ Responsabilidad: Static File Server + SPA Routing             │
│                                                                │
│ - try_files $uri /index.html → Vue Router no rompe en refresh │
│ - /api/* → proxy_pass al Gateway (sin exponer microservicios) │
│ - Servir assets estáticos (Nginx es lo más rápido para esto)  │
│                                                                │
│ ¿Se puede quitar? Sin Nginx, el SPA no funciona: al        │
│ refrescar /dashboard, no hay archivo físico y retorna 404.    │
│ YARP no tiene try_files nativo. Tendríamos que implementarlo  │
│ con middleware custom.                                         │
└──────────────────────────┬───────────────────────────────────┘
                           │ /api/*
                           ▼
┌──────────────────────────────────────────────────────────────┐
│ YARP (API Gateway)                                             │
│ Responsabilidad: API Gateway — Service Routing + Health       │
│                                                                │
│ - /auth/* → iam:5001                                          │
│ - /devices/* → devices:5002                                   │
│ - /projects/* → projects:5003                                 │
│ - GET /health → estado agregado de los 5 microservicios       │
│ - CORS centralizado (una política, no 5)                      │
│ - Mismo stack .NET, mismo pipeline CI/CD                      │
│                                                                │
│ ¿Se puede quitar? Técnicamente sí. Nginx podría rutear     │
│ directo a cada microservicio. Pero perderíamos: health checks │
│ agregados, CORS centralizado, y el patrón API Gateway como    │
│ ADR documentado.                                               │
└──────────────────────────────────────────────────────────────┘
```

#### ¿Por Qué No Consolidar?

| Estrategia de consolidación | ¿Qué se pierde? |
|---------------------------|-----------------|
| **Quitar Traefik** | Habría que manejar certificados SSL a mano con certbot + cron. |
| **Quitar Nginx, que YARP sirva el frontend** | YARP no tiene `try_files` para SPA. Habría que escribir middleware custom. Nginx es más rápido para estáticos. |
| **Quitar YARP, que Nginx rutee directo a microservicios** | Sin health checks agregados. Si un microservicio falla, Nginx no lo sabe. CORS se configura en 5 lugares distintos. |
| **Quitar Nginx Y YARP, que Traefik haga todo** | Traefik no es servidor de archivos estáticos. No tiene `try_files` para SPA. Perdemos health checks a nivel aplicación (.NET). |

#### ¿Es Esto Sobre-Ingeniería?

**No para un sistema de microservicios en producción.** En un monolito con un solo deploy, Traefik + Nginx serían suficientes. Pero en nuestra arquitectura:

1. **Traefik** resuelve un problema que EXISTE en cualquier deploy real: TLS. No es opcional tener HTTPS en producción.

2. **Nginx** resuelve un problema CONCRETO del stack elegido: Vue.js es una SPA, y las SPAs necesitan `try_files` para que las rutas del cliente no retornen 404. No es un lujo, es un requisito.

3. **YARP** agrega observabilidad y separación de concerns a nivel aplicación. Cuesta 128 MB de RAM. Para una app que ya consume ~1.3 GB en total, ese overhead es marginal y el beneficio en operabilidad es real.

#### Trade-offs Aceptados

| Trade-off | Mitigación |
|-----------|-----------|
| **Complejidad percibida** (3 proxies) | Cada uno tiene una responsabilidad distinta y documentada. No hay redundancia funcional. |
| **Latencia adicional** (3 saltos HTTP internos) | Todos los saltos son en localhost/Docker network. Latencia < 1ms por salto. Imperceptible. |
| **Mayor superficie de configuración** | Dos de los tres (Traefik, Nginx) son estándar de industria. Cualquier developer los conoce. |
| **Mayor consumo de memoria** (+128 MB YARP, +50 MB Nginx) | Un VPS minimo tiene +2 GB. El overhead total de proxies es <200 MB. Aceptable. |

#### Evolución en Siguientes Iteraciones

Este "problema" de tres proxies **no aplica como urgencia en la Iteración 1** porque:

1. La escala actual (8 contenedores, 1 dominio) no justifica optimizar el pipeline de proxies.
2. El foco de la Iteración 1 fue **seguridad, estructura base y despliegue**. Los proxies son un medio, no el fin.
3. La decisión de mantener los tres fue deliberada para **no acoplar responsabilidades prematuramente**.

**En las iteraciones futuras fueran hipoteticamente:**

| Iteración | Posible cambio | Gatillo |
|-----------|---------------|--------|
| **Iteración 2** | Consolidar YARP + Nginx si se justifica con benchmarks | Si la latencia de 3 saltos se vuelve medible (>5ms) |
| **Iteración 3** | Reemplazar YARP con NGINX sidecar si se adopta service mesh | Si migramos a Kubernetes con Istio/Linkerd |
| **Iteración 3** | Exponer YARP directamente vía subdominio `api.dominio.com` para clientes no-web | Si se agrega una app mobile que necesita acceso directo a APIs sin pasar por Nginx |
| **Iteración 4** | Agregar MQTT broker para telemetría IoT, bypass completo de HTTP | Si Devices escala a miles de sensores con alta frecuencia de datos |

> **Principio rector:** No optimizar prematuramente. La arquitectura actual es correcta para la escala y requisitos de la Iteración 1. Si un componente se vuelve un bottleneck real, lo abordamos con datos, no con intuición.

---

## 9. La "Magia" de Transformar un Monolito: Justificación de Cada Decisión

### 9.1 ¿Por qué Microservicios y no un Monolito Modular?

**Argumento común:** "Un monolito bien modularizado con bounded contexts en diferentes namespaces resuelve los mismos problemas sin la complejidad de red."

**Nuestra realidad en IoBuild:**
- **Escalabilidad independiente:** Devices va a recibir cientos de eventos de telemetría por segundo. Projects recibe unos pocos CRUD por minuto. Con un monolito, todos escalan juntos o nada.
- **Aislamiento de fallos:** Si Stripe (Subscriptions) tiene una outage, en un monolito toda la app cae. En microservicios, solo Subscriptions falla.
- **Equipos:** Aunque ahora somos un equipo chico, la arquitectura está lista para que múltiples equipos trabajen en paralelo sin pisarse.
- **Deploy independiente:** Podemos desplegar una corrección en IAM sin recompilar ni redesplegar todo el sistema.

### 9.2 ¿Por qué Clean Architecture y no Minimal API directo?

**Contexto:** .NET 9 soporta Minimal APIs donde ponés todo en `Program.cs`.

**Nuestra decisión:** Clean Architecture en 4 capas.

**Justificación:**
- **Testeabilidad:** Los tests BDD mockean interfaces del Domain. Sin capas, no hay qué mockear.
- **Independencia de frameworks:** Si EF Core lanza una versión breaking, solo tocamos Infrastructure. El Domain no se entera.
- **Onboarding:** Un desarrollador nuevo abre `IoBuild.Projects/` y ve 4 carpetas claras: `Interfaces/`, `Application/`, `Domain/`, `Infrastructure/`. Sabe exactamente dónde está cada cosa.
- **Migración futura:** Si el día de mañana queremos cambiar Subscriptions a una arquitectura event-driven, el Domain se reusa. Solo cambia Infrastructure.

### 9.3 ¿Por qué CQRS desde el Día 1?

**Argumento común:** "CQRS añade complejidad. No lo necesitás hasta que tengás problemas de performance."

**Nuestra realidad:**
- **Claridad conceptual:** `CreateProjectCommand` + `ProjectCommandService` es más expresivo que `ProjectService.Create()`. El código documenta la intención.
- **Preparación para el futuro:** Cuando Devices reciba telemetría masiva, podemos separar la BD de lectura (réplica) de la de escritura sin cambiar el Domain.
- **Costo mínimo:** Separar `CommandServices/` de `QueryServices/` es solo organización de archivos. No estamos implementando Event Sourcing ni bases de datos separadas (todavía).

### 9.4 ¿Por qué YARP y no Ocelot?

| Opción | Pro | Contra | Veredicto |
|--------|-----|--------|-----------|
| **YARP** | Nativo .NET, config en JSON, health checks integrados, mismo pipeline CI/CD | Menos maduro que NGINX, comunidad más chica | ✅ Elegido |
| **Ocelot** | Popular, buena documentación | Performance inferior a YARP, configuración más verbosa | ❌ |

**YARP ganó porque:** Todo nuestro stack es .NET 9. Un solo ecosistema, un solo pipeline de build, un solo tipo de health check, una sola forma de configurar (JSON).

### 9.5 ¿Por qué Traefik (vía Dokploy) y no NGINX como Proxy Externo?

**NGINX:** Requiere archivos `.conf` que no se auto-actualizan cuando añadís contenedores. Cada nuevo servicio requiere editar la config a mano.

**Traefik:** Descubre contenedores automáticamente vía labels de Docker. Dokploy lo configura por vos cuando añadís un dominio desde la UI. Let's Encrypt integrado sin configuración manual.

**Conclusión:** Para un VPS pequeño con pocos servicios, Traefik + Dokploy reduce dramáticamente la carga operativa versus administrar NGINX a mano.

### 9.6 ¿Por qué MySQL en Contenedor y no Servicio Gestionado?

Para un MVP con 2 GB de RAM, un servicio gestionado (AWS RDS, Google Cloud SQL) cuesta mínimo $15-30/mes adicionales y añade latencia de red.

MySQL en contenedor:
- Cero costo adicional.
- Sin latencia de red (localhost dentro de la red Docker).
- Fácil backup con bind mounts.
- Migrable a RDS en el futuro sin cambiar código (solo connection string).



---

## 10. Línea de Tiempo de Commits

| # | Commit | Fase | Descripción |
|---|--------|------|------------|
| 1 | `f3f5168` | Inicio | First commit — estructura base |A
| 2 | `873e943` | Fase 1 | Iteration 1: 7 proyectos .NET, Gateway, Shared Library |
| 3 | `a893f4b` | Fase 1 | Update .gitignore |
| 4 | `2e63d6b` | Fase 1 | Fix: corrección de nombre de archivo |
| 5 | `9f27520` | Fase 2 | BDD testing inicial |
| 6 | `55e1803` | Fase 2 | Integration tests + más BDD |
| 7 | `a9de98d` | Fase 3 | Dockerize all backend microservices |
| 8 | `e31b9ef` | Fase 3 | Add frontend to docker environment |
| 9 | `b2b60a5` | Fase 3 | Implement docker compose for production |
| 10 | `1065bdc` | Fase 4 | Fix: deploy dockerizado — submodule, npm ci, env vars, override |
| 11 | `f5596b5` | Fase 4 | Fix: arranque secuencial + límites de memoria para VPS |
| 12 | `c29f710` | Fase 4 | Fix: remover puerto 80 del compose base (Traefik) |
| 13 | `5853c67` | Fase 4 | Fix: usar curl en vez de wget para health check (nginx:alpine) |

**13 commits. 4 fases. Del monolito a producción.**

---

## 11. Estadísticas Finales

| Métrica | Valor |
|---------|-------|
| **Proyectos .NET fuente** | 7 (1 Shared + 5 Microservicios + 1 Gateway) |
| **Proyectos de Test** | 4 (xUnit + SpecFlow) |
| **Archivos .cs totales** | ~125 fuente + ~20 test |
| **Dockerfiles** | 8 (6 backend + 1 gateway + 1 frontend) |
| **Contenedores en producción** | 8 (MySQL + 5 servicios + Gateway + Frontend) |
| **Bases de datos** | 5 (una por microservicio) |
| **Escenarios BDD** | 16 (todos con Step Definitions implementados) |
| **Integration Tests runtime** | 10 |
| **Tests totales pasando** | 26/26 (100%) |
| **Patrones GoF implementados** | 11 (DI, Facade, Adapter, Chain of Resp., Repository, Strategy, Mediator, Decorator, Unit of Work, Proxy (Gateway), Aggregate Root) |
| **Principios SOLID aplicados** | 5/5 (SRP, OCP, LSP, ISP, DIP) |
| **Drivers ADD cubiertos** | 4/4 (CRN-1, QA-1, CON-1, CON-2) |
| **ADRs documentados** | 8 |
| **Puertos en uso** | 8080 (Gateway), 5001-5005 (servicios), 80 (Frontend/Nginx) |
| **Rutas API expuestas** | 30 endpoints (5 autenticación + 5 dispositivos + 7 proyectos + 6 suscripciones + 2 analytics) |
| **Bugs en producción resueltos** | 5 (submodule, memoria, puerto, health check, dominio) |
| **Commits totales** | 13 |
| **Horas estimadas de trabajo** | ~40-50 horas |
| **Tamaño total de imágenes Docker** | ~1.3 GB (8 imágenes) |

---

## 12. Arquitectura Final de Producción

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        INTERNET                                          │
│                     io-build-v2.arroz.dev                                │
└────────────────────────────────┬────────────────────────────────────────┘
                                 │ HTTPS :443
                                 ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                          TRAEFIK                            │
│                    TLS (Let's Encrypt) + Routing                         │
└────────────────────────────────┬────────────────────────────────────────┘
                                 │ HTTP :80 (interno)
                                 ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                     FRONTEND (Nginx :80)                                  │
│                                                                          │
│  /                 → SPA (Vue 3 — index.html)                            │
│  /api/*            → proxy_pass http://gateway:8080                      │
└────────────────────────────────┬────────────────────────────────────────┘
                                 │ /api/v1/*
                                 ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                    API GATEWAY (YARP :8080)                               │
│                                                                          │
│  /api/v1/authentication/*  → iam:5001                                    │
│  /api/v1/users/*           → iam:5001                                    │
│  /api/v1/devices/*         → devices:5002                                │
│  /api/v1/projects/*        → projects:5003                               │
│  /api/v1/units/*           → projects:5003                               │
│  /api/v1/clients/*         → projects:5003                               │
│  /api/v1/subscriptions/*   → subscriptions:5004                          │
│  /api/v1/plans/*           → subscriptions:5004                          │
│  /api/v1/analytics/*       → analytics:5005                              │
│  GET /health → Estado de todos los servicios                             │
└────┬──────────┬──────────┬──────────┬──────────┐
     │          │          │          │          │
     ▼          ▼          ▼          ▼          ▼
┌─────────┐┌─────────┐┌─────────┐┌─────────┐┌─────────┐
│   IAM   ││ DEVICES ││PROJECTS ││  SUBS   ││ANALYTICS│
│  :5001  ││  :5002  ││  :5003  ││  :5004  ││  :5005  │
└────┬────┘└────┬────┘└────┬────┘└────┬────┘└────┬────┘
     │          │          │          │          │
     ▼          ▼          ▼          ▼          ▼
┌─────────────────────────────────────────────────────────┐
│                    MySQL 8.0 (:3306)                     │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐   │
│  │iobuild   │ │iobuild   │ │iobuild   │ │iobuild   │...│
│  │_iam      │ │_devices  │ │_projects │ │_subs     │   │
│  └──────────┘ └──────────┘ └──────────┘ └──────────┘   │
└─────────────────────────────────────────────────────────┘
```

---

## 13. Lecciones Aprendidas

### 12.1 Sobre Arquitectura

1. **El monolito no es el enemigo.** El monolito funcionaba. La decisión de migrar a microservicios fue impulsada por requisitos no funcionales (escalabilidad, aislamiento de fallos, deploy independiente) y por los drivers arquitectónicos del curso. No "arreglamos" un monolito roto — **evolucionamos** una arquitectura que ya no escalaba a las necesidades del dominio.

2. **La librería compartida (`IoBuild.Shared`) fue la decisión más rentable del proyecto.** Evitó duplicar middleware, convenciones, y configuración en 5 microservicios. El retorno de inversión fue inmediato: cada fix de seguridad se aplicaba en un solo lugar.

3. **Clean Architecture paga su "costo" en la primera semana.** Los tests BDD se escribieron en minutos porque cada capa tiene interfaces mockeables. Sin eso, habríamos necesitado integración real con BD para cada test.

### 12.2 Sobre Testing

4. **Los integration tests runtime encontraron bugs que los tests BDD con mocks no podían encontrar.** Un middleware mal configurado, un `[Authorize]` faltante, un código HTTP incorrecto — todo eso solo se detecta pegándole a la API real.

5. **BDD con SpecFlow fue útil, pero requiere disciplina.** Los escenarios Gherkin son excelentes para documentar comportamiento, pero si no se implementan los Step Definitions, son solo comentarios bonitos.

### 12.3 Sobre Docker y DevOps

6. **Docker Compose es suficiente para la mayoría de los proyectos.** No todo necesita Kubernetes. Para 8 contenedores en un VPS, Compose + Dokploy da orquestación, health checks, reinicio automático y despliegue continuo sin la complejidad operativa de K8s.

7. **Los health checks no son opcionales.** Sin health checks, los contenedores arrancan en orden aleatorio y las dependencias fallan. Con health checks, cada servicio espera a que el anterior esté realmente listo (no solo el puerto abierto).

8. **El bug más difícil fue el más simple:** `wget --spider` no funciona en BusyBox. Una línea de `curl` resolvió un día entero de troubleshooting. Siempre verificá que las herramientas que usás en health checks existan en la imagen base.

### 12.4 Sobre Producción

9. **Dokploy abstrae Traefik, pero no es magia.** Cuando un dominio no funciona, hay que entender qué está pasando debajo: ¿está el contenedor healthy? ¿está en la red correcta? ¿el puerto del health check es el mismo que el puerto del servicio? Dokploy es la capa de arriba — el diagnóstico requiere bajar a Docker.

10. **Nunca expongas puertos de backend al host en producción.** `expose`, no `ports`. En desarrollo local sí exponelos (`docker-compose.override.yml`) para debuggear. Pero en producción, solo Traefik debe poder acceder a los servicios.

---

## Conclusión

Lo que empezó como un monolito heredado con ~10 archivos en un solo proyecto terminó como una arquitectura de microservicios con:

- **7 proyectos .NET** en Clean Architecture de 4 capas
- **11 patrones GoF** aplicados con justificación arquitectónica
- **5 bases de datos independientes** por Bounded Context
- **26 tests** entre BDD e integración (100% passing)
- **8 contenedores Docker** orquestados con Compose
- **Despliegue automático** en VPS con Dokploy + Traefik + Let's Encrypt
- **App funcionando** en `https://io-build-v2.arroz.dev`

---

> **Documento generado para el curso de Fundamentos de Arquitectura de Software — UPC**
>
> **Proyecto:** IoBuild — Iteración 1 (Cierre Final)
> **Fecha:** Mayo 2026
> **Estado:**  Desplegado en Producción
