# IoBuild — Plataforma de Gestión de Propiedades e IoT

## Arquitectura de Microservicios

> **Curso:** Fundamentos de Arquitectura de Software — UPC  
> **Proyecto:** IoBuild — Sistema de construcción inteligente  
> **Iteración actual:** 1 (Base Arquitectónica + Seguridad)

---

## Documentación Disponible

| Documento | Descripción |
|-----------|-------------|
| [`docs/architecture-overview.md`](docs/architecture-overview.md) | **Documento principal** — Explica qué, cómo, resultado y beneficios |
| [`docs/api-gateway-routes.md`](docs/api-gateway-routes.md) | Mapeo detallado de rutas del API Gateway |
| [`docs/iteration_1_imp.md`](docs/iteration_1_imp.md) | Reporte técnico de implementación con estadísticas |
| [`docs/iteration-1-closure.md`](docs/iteration-1-closure.md) | Cierre formal de Iteración 1 (quality gate) |

---

## Inicio Rápido

### Requisitos previos
- .NET 9 SDK
- MySQL corriendo
- **Configurar variables de entorno** (o copiar `.env`):

```bash
# Copiar el template de variables de entorno
cp .env.example .env
# Editar .env.local con tus credenciales reales (DB, JWT, Stripe)
```

### Paso a paso

```bash
# 1. Bases de datos
mysql -u root -h 127.0.0.1 -P 3306 -e "
  CREATE DATABASE IF NOT EXISTS iobuild_iam;
  CREATE DATABASE IF NOT EXISTS iobuild_devices;
  CREATE DATABASE IF NOT EXISTS iobuild_projects;
  CREATE DATABASE IF NOT EXISTS iobuild_subscriptions;
  CREATE DATABASE IF NOT EXISTS iobuild_analytics;
"

# 2. Variables de entorno
export DB_HOST=127.0.0.1 DB_PORT=3306 DB_USER=root DB_PASSWORD=[password] JWT_SECRET="mi-secreto"

# 3. Gateway (primero)
dotnet run --project src/IoBuild.Gateway

# 4. Microservicios (terminales separadas)
DB_NAME=iobuild_iam          dotnet run --project src/IoBuild.IAM
DB_NAME=iobuild_devices      dotnet run --project src/IoBuild.Devices
DB_NAME=iobuild_projects     dotnet run --project src/IoBuild.Projects
DB_NAME=iobuild_subscriptions dotnet run --project src/IoBuild.Subscriptions
DB_NAME=iobuild_analytics    dotnet run --project src/IoBuild.Analytics

# 5. Verificar
curl http://localhost:8080/health
```

---

## Estructura

```
microservices/
├── IoBuild.sln                    → Solución .NET 9
├── README.md                      → Este archivo
├── start_services.ps1             → Script para iniciar todo
├── Docs/
│   ├── architecture-overview.md   → Visión general de la arquitectura
│   ├── api-gateway-routes.md      → Enrutamiento del Gateway
│   ├── iteration_1_imp.md         → Reporte de implementación
│   └── iteration-1-closure.md     → Cierre formal de Iteración 1
├── src/
│   ├── IoBuild.Shared/            → Librería compartida
│   ├── IoBuild.IAM/               → Auth (puerto 5001)
│   ├── IoBuild.Devices/           → IoT (puerto 5002)
│   ├── IoBuild.Projects/          → Proyectos (puerto 5003)
│   ├── IoBuild.Subscriptions/     → Pagos (puerto 5004)
│   ├── IoBuild.Analytics/         → Dashboards (puerto 5005)
│   └── IoBuild.Gateway/           → Gateway YARP (puerto 8080)
└── tests/
    ├── IoBuild.IAM.Tests/         → BDD: Authentication
    ├── IoBuild.Devices.Tests/     → BDD: DeviceManagement
    ├── IoBuild.Projects.Tests/    → BDD: ProjectsManagement
    └── IoBuild.Subscriptions.Tests/ → BDD: SubscriptionRenewal
```

---

## Estado Actual

| Servicio | Puerto | Estado |
|----------|--------|--------|
| Gateway | 8080 | ✅ Healthy |
| IAM | 5001 | ✅ Healthy |
| Devices | 5002 | ✅ Healthy |
| Projects | 5003 | ✅ Healthy |
| Subscriptions | 5004 | ✅ Healthy |
| Analytics | 5005 | ✅ Healthy |

**Pruebas validadas:** 10/10 exitosas  
**Escenarios BDD:** 12 (4 Features)  
**Errores de compilación:** 0  
**Archivos fuente:** ~170 .cs
