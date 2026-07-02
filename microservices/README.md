# IoBuild — Sistema de Gestión de Propiedades e IoT

> **Curso:** Fundamentos de Arquitectura de Software — UPC  
> **Proyecto:** IoBuild — Plataforma de construcción inteligente con monitoreo IoT  
> **Arquitectura:** Microservicios con Clean Architecture por Bounded Context

---

## ¿Qué es IoBuild?

IoBuild es una plataforma B2B/B2C para gestión de proyectos de construcción con monitoreo IoT en tiempo real.  
El sistema fue **migrado de un monolito heredado** (curso Aplicaciones Web) hacia una arquitectura de microservicios completa, documentando cada decisión arquitectónica a través de tres iteraciones ADD.

---

## Inicio Rápido

### Requisitos
- .NET 9 SDK
- MySQL corriendo en `:33065`
- Docker (para el entorno completo con IoT)

### Con Docker (recomendado)

```bash
# Clonar y levantar todo el stack
docker compose up --build

# Verificar que todo esté healthy
curl http://localhost:8080/health
```

### Sin Docker (desarrollo local)

```bash
# 1. Crear bases de datos
mysql -u root -h 127.0.0.1 -P 33065 -e "
  CREATE DATABASE IF NOT EXISTS iobuild_iam;
  CREATE DATABASE IF NOT EXISTS iobuild_devices;
  CREATE DATABASE IF NOT EXISTS iobuild_projects;
  CREATE DATABASE IF NOT EXISTS iobuild_subscriptions;
  CREATE DATABASE IF NOT EXISTS iobuild_analytics;
  CREATE DATABASE IF NOT EXISTS iobuild_profiles;
"

# 2. Iniciar servicios (terminales separadas)
dotnet run --project src/IoBuild.Gateway
DB_NAME=iobuild_iam           dotnet run --project src/IoBuild.IAM
DB_NAME=iobuild_devices       dotnet run --project src/IoBuild.Devices
DB_NAME=iobuild_projects      dotnet run --project src/IoBuild.Projects
DB_NAME=iobuild_subscriptions dotnet run --project src/IoBuild.Subscriptions
DB_NAME=iobuild_analytics     dotnet run --project src/IoBuild.Analytics
DB_NAME=iobuild_profiles      dotnet run --project src/IoBuild.Profiles

# 3. Ejecutar integration tests
./run_integration_tests.sh
```

---

## Estado Actual del Sistema

| Servicio | Puerto | Bounded Context | Estado |
|----------|--------|----------------|--------|
| Gateway (YARP) | 8080 | — (Routing) | ✅ Healthy |
| IAM | 5001 | Identidad y Acceso | ✅ Healthy |
| Devices | 5002 | Dispositivos IoT + Telemetría | ✅ Healthy |
| Projects | 5003 | Proyectos, Unidades, Clientes | ✅ Healthy |
| Subscriptions | 5004 | Planes, Pagos Stripe, Outbox | ✅ Healthy |
| Analytics | 5005 | Dashboards y Métricas | ✅ Healthy |
| Profiles | 5006 | Perfiles de Usuario | ✅ Healthy |
| Frontend (Nginx) | 80 | Vue 3 SPA | ✅ Healthy |
| MySQL | 3307-3312 | Persistencia (6 contenedores, 1 por servicio) | ✅ Healthy |
| InfluxDB | 8086 | Telemetría IoT (7 días retención) | ✅ Healthy |
| Mosquitto | 1883 | MQTT Broker | ✅ Healthy |
| RabbitMQ | 5672/15672 | Broker de eventos (Outbox Pattern) | ✅ Healthy |
| Redis | 6379 | Token blacklist distribuido (revocación JWT) | ✅ Healthy |
| Jaeger | 16686 | Tracing distribuido (OpenTelemetry) | ✅ Healthy |
| IoT Simulator | — | Generador de datos (dev only) | ✅ Running |

---

## Estructura del Repositorio

```
microservices/
├── IoBuild.sln                          # Solución .NET 9
├── docker-compose.yml                   # Stack completo (desarrollo)
├── docker-compose.prod.yml              # Stack de producción (GHCR images)
├── docker-compose.override.yml          # Ports locales para desarrollo
├── run_integration_tests.sh             # Integration tests (Bash)
├── run_integration_tests.ps1            # Integration tests (PowerShell)
├── start_all.sh / kill_all.sh          # Ciclo de vida de servicios
├── docs/                                # ← Toda la documentación del proyecto
│   ├── architecture/                    # Vista arquitectónica actual
│   ├── iterations/                      # Reportes por iteración ADD
│   ├── migrations/                      # Cambios de soporte (no iteraciones ADD)
│   └── testing/                         # Evidencia de testing
├── src/
│   ├── IoBuild.Shared/                  # Librería transversal (JWT, middleware, base repos)
│   ├── IoBuild.IAM/                     # Auth, JWT, roles
│   ├── IoBuild.Devices/                 # CRUD IoT + Pipeline MQTT→InfluxDB
│   ├── IoBuild.Projects/                # Proyectos, Unidades, Clientes
│   ├── IoBuild.Subscriptions/           # Planes, Stripe, Outbox Pattern
│   ├── IoBuild.Analytics/               # Dashboards y métricas
│   ├── IoBuild.Profiles/                # Perfiles de usuario
│   └── IoBuild.Gateway/                 # API Gateway (YARP)
├── tests/
│   ├── IoBuild.IAM.Tests/               # BDD: Authentication (4 escenarios)
│   ├── IoBuild.Devices.Tests/           # BDD: DeviceManagement + Telemetry (8 escenarios)
│   ├── IoBuild.Projects.Tests/          # BDD: ProjectsManagement (4 escenarios)
│   └── IoBuild.Subscriptions.Tests/     # BDD: SubscriptionRenewal + Outbox (4+ escenarios)
├── mosquitto/                           # Config del broker MQTT
├── mysql/                               # Scripts de inicialización de bases de datos (1 por servicio)
├── iot-simulator/                       # Simulador IoT en Python
└── frontend-docker/                     # Build Docker del frontend (Nginx)
```

---

## Documentación

| Documento | Descripción |
|-----------|-------------|
| [`docs/architecture/overview.md`](docs/architecture/overview.md) | **Vista arquitectónica actual** — estado real del sistema validado contra el código |
| [`docs/architecture/api-gateway-routes.md`](docs/architecture/api-gateway-routes.md) | Mapeo completo de rutas del Gateway |
| [`docs/iterations/iteration-1-base-seguridad.md`](docs/iterations/iteration-1-base-seguridad.md) | Iteración 1: Del monolito a microservicios + seguridad JWT + despliegue |
| [`docs/iterations/iteration-2-pipeline-iot.md`](docs/iterations/iteration-2-pipeline-iot.md) | Iteración 2: Pipeline MQTT + InfluxDB + telemetría en tiempo real |
| [`docs/iterations/iteration-3-pagos-seguros.md`](docs/iterations/iteration-3-pagos-seguros.md) | Iteración 3: Pagos seguros con Stripe, Outbox y Idempotency Keys |
| [`docs/iterations/iteration-4-observabilidad.md`](docs/iterations/iteration-4-observabilidad.md) | Iteración 4: Tracing distribuido con OpenTelemetry + Jaeger |
| [`docs/migrations/support-migrations.md`](docs/migrations/support-migrations.md) | Migraciones de soporte — cambios fuera del scope de las iteraciones ADD |
| [`docs/testing/iteration-1-evidence.md`](docs/testing/iteration-1-evidence.md) | Evidencia de testing completa de la Iteración 1 |
| [`docs/deployment/azure-terraform.md`](docs/deployment/azure-terraform.md) | **Deploy actual** — Azure VM efímera con Terraform + Cloudflare |

---

> **Proyecto académico — Fundamentos de Arquitectura de Software — UPC**  
> Del monolito heredado a una arquitectura de microservicios en producción.
