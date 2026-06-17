# API Gateway — Enrutamiento de Microservicios IoBuild

> Validado contra `src/IoBuild.Gateway/appsettings.json`.

## ¿Qué es el API Gateway?

El Gateway es el **único punto de entrada** al sistema. Todo request del frontend (Vue App) o cliente externo pasa por él. Su trabajo es:

1. **Enrutar** cada path al microservicio correcto
2. **Monitorear** la salud de todos los servicios
3. **Centralizar** CORS, logging y manejo de errores

**Tecnología:** YARP (Yet Another Reverse Proxy) — proxy nativo de Microsoft para .NET  
**Puerto:** `8080`

---

## Topología

```
Cliente (Vue Web App :5173 o curl/Postman)
       │
       ▼
┌─────────────────────────────────────────────────────────────┐
│                      API GATEWAY (:8080)                      │
│                                                               │
│  GET / → Status del Gateway                                  │
│  GET /health → Health Check de todos los microservicios      │
│                                                               │
│  /api/v1/authentication/*     ──→ IoBuild.IAM       (:5001)  │
│  /api/v1/users/*              ──→ IoBuild.IAM       (:5001)  │
│  /api/v1/profiles/*           ──→ IoBuild.Profiles  (:5006)  │
│  /api/v1/devices/*            ──→ IoBuild.Devices   (:5002)  │
│  /api/v1/projects/*           ──→ IoBuild.Projects  (:5003)  │
│  /api/v1/units/*              ──→ IoBuild.Projects  (:5003)  │
│  /api/v1/clients/*            ──→ IoBuild.Projects  (:5003)  │
│  /api/v1/subscriptions/*      ──→ IoBuild.Subscriptions(:5004)│
│  /api/v1/plans/*              ──→ IoBuild.Subscriptions(:5004)│
│  /api/v1/webhooks/*           ──→ IoBuild.Subscriptions(:5004)│
│  /api/v1/analytics/*          ──→ IoBuild.Analytics  (:5005) │
└─────────────────────────────────────────────────────────────┘
```

---

## Mapeo Completo de Rutas

| Ruta Pública (Gateway :8080) | Microservicio Destino | Puerto | Cluster YARP |
|------------------------------|----------------------|--------|-------------|
| `/api/v1/authentication/*` | IoBuild.IAM | 5001 | `iam-cluster` |
| `/api/v1/users/*` | IoBuild.IAM | 5001 | `iam-cluster` |
| `/api/v1/profiles/*` | IoBuild.Profiles | 5006 | `profiles-cluster` |
| `/api/v1/devices/*` | IoBuild.Devices | 5002 | `devices-cluster` |
| `/api/v1/projects/*` | IoBuild.Projects | 5003 | `projects-cluster` |
| `/api/v1/units/*` | IoBuild.Projects | 5003 | `projects-cluster` |
| `/api/v1/clients/*` | IoBuild.Projects | 5003 | `projects-cluster` |
| `/api/v1/subscriptions/*` | IoBuild.Subscriptions | 5004 | `subscriptions-cluster` |
| `/api/v1/plans/*` | IoBuild.Subscriptions | 5004 | `subscriptions-cluster` |
| `/api/v1/webhooks/*` | IoBuild.Subscriptions | 5004 | `subscriptions-cluster` |
| `/api/v1/analytics/*` | IoBuild.Analytics | 5005 | `analytics-cluster` |

---

## Cómo funciona internamente (YARP)

El Gateway usa **YARP Reverse Proxy** configurado vía `appsettings.json`. Define dos conceptos:

### Routes (Rutas)
Cada ruta es una regla que dice "si el path coincide, redirige al cluster X":

```json
"iam-auth": {
  "ClusterId": "iam-cluster",
  "Match": { "Path": "/api/v1/authentication/{**catch-all}" }
}
```

### Clusters (Destinos)
Cada cluster define a dónde enviar las peticiones, con política de balanceo y health check:

```json
"profiles-cluster": {
  "LoadBalancingPolicy": "RoundRobin",
  "Destinations": {
    "profiles-destination": { "Address": "http://localhost:5006/" }
  },
  "HealthCheck": {
    "Active": {
      "Enabled": true,
      "Interval": "00:00:10",
      "Timeout": "00:00:05",
      "Policy": "ConsecutiveFailures",
      "Path": "/health"
    }
  }
}
```

### Pipeline completo de un request

```
Request entrante
  │
  ▼
GlobalExceptionHandler   ← Captura errores globales
  │
  ▼
CORS Policy              ← Permite/deniega orígenes
  │
  ▼
Health Checks (/health)  ← Monitorea estado de servicios
  │
  ▼
YARP Reverse Proxy       ← Enruta al microservicio correcto
  │
  ▼
Microservicio destino (:5001-5006)
  │
  ▼
GlobalExceptionHandler   ← Captura errores del servicio
  │
  ▼
Auth Middleware (si aplica) ← Valida JWT
  │
  ▼
Controller → Service → Repository → MySQL / InfluxDB
```

---

## Health Checks

El Gateway monitorea activamente la salud de **todos los microservicios** cada 10 segundos.

```bash
curl http://localhost:8080/health
```

**Respuesta:**
```json
{
  "status": "Healthy",
  "services": {
    "IoBuild.IAM":          { "status": "Healthy" },
    "IoBuild.Devices":      { "status": "Healthy" },
    "IoBuild.Projects":     { "status": "Healthy" },
    "IoBuild.Subscriptions": { "status": "Healthy" },
    "IoBuild.Analytics":    { "status": "Healthy" },
    "IoBuild.Profiles":     { "status": "Healthy" }
  }
}
```

Cada microservicio también expone su propio `/health`:

```bash
curl http://localhost:5001/health   # IAM
curl http://localhost:5002/health   # Devices
curl http://localhost:5003/health   # Projects
curl http://localhost:5004/health   # Subscriptions
curl http://localhost:5005/health   # Analytics
curl http://localhost:5006/health   # Profiles
```

---

## Resiliencia

| Característica | Configuración |
|---------------|--------------|
| **Balanceo** | RoundRobin (distribuye entre réplicas si hubiera) |
| **Health Check activo** | Cada 10s, timeout 5s, política `ConsecutiveFailures` |
| **Escalado horizontal** | Agregar más destinos al cluster = más réplicas |

---

## Seguridad

El Gateway **no valida JWT** — eso lo hace cada microservicio individualmente mediante el middleware de auth de `IoBuild.Shared`. El Gateway provee:

- **CORS** centralizado (una política para todos los servicios)
- **GlobalExceptionHandler** — evita que errores internos lleguen al cliente
- **Health checks activos** — detecta servicios caídos y deja de enrutarles tráfico

---

## ¿Qué pasa si un microservicio se cae?

Los demás siguen funcionando. Ejemplo:

```
Si IoBuild.Devices (:5002) se cae:
  ✅ /api/v1/authentication/sign-in  → funciona (IAM está vivo)
  ✅ /api/v1/projects                 → funciona (Projects está vivo)
  ✅ /api/v1/profiles                 → funciona (Profiles está vivo)
  ❌ /api/v1/devices                  → 502 Bad Gateway
  ⚠️ GET /health                      → "Unhealthy" con Devices en rojo
```

Esto es **aislamiento de fallos** — un beneficio clave de la arquitectura de microservicios.

---

## Opciones Evaluadas

| Opción | Ventajas | Desventajas | Decisión |
|--------|----------|------------|---------|
| **YARP** (usado) | Nativo .NET, config JSON, health checks integrados, mismo CI/CD | Comunidad más chica que Nginx | ✅ Elegido |
| **Ocelot** | Gateway maduro, buena documentación | Performance inferior, más verboso | ❌ |
| **Kong / Nginx** | Extremadamente maduro, plugins, rate limiting | No es .NET, requiere infraestructura adicional | ❌ |
