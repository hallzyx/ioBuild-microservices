# API Gateway — Enrutamiento de Microservicios IoBuild

## ¿Qué es el API Gateway?

El Gateway es el **único punto de entrada** al sistema. Todo request del frontend (Vue App) o cliente externo pasa por él. Su trabajo es:

1. **Enrutar** cada path al microservicio correcto
2. **Monitorear** la salud de todos los servicios
3. **Centralizar** CORS, logging, y manejo de errores

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
│  GET /health → Health Check de los 5 microservicios          │
│                                                               │
│  /api/v1/authentication/*     ──→ IoBuild.IAM       (:5001)  │
│  /api/v1/users/*              ──→ IoBuild.IAM       (:5001)  │
│  /api/v1/profiles/*           ──→ IoBuild.IAM       (:5001)  │
│  /api/v1/devices/*            ──→ IoBuild.Devices   (:5002)  │
│  /api/v1/projects/*           ──→ IoBuild.Projects  (:5003)  │
│  /api/v1/units/*              ──→ IoBuild.Projects  (:5003)  │
│  /api/v1/clients/*            ──→ IoBuild.Projects  (:5003)  │
│  /api/v1/subscriptions/*      ──→ IoBuild.Subscription(:5004)│
│  /api/v1/plans/*              ──→ IoBuild.Subscription(:5004)│
│  /api/v1/analytics/*          ──→ IoBuild.Analytics  (:5005) │
└─────────────────────────────────────────────────────────────┘
```

---

## Mapeo Completo de Rutas

| Ruta Pública (Gateway :8080) | Microservicio Destino | Puerto | Endpoints que expone |
|------------------------------|----------------------|--------|---------------------|
| `/api/v1/authentication/*` | IoBuild.IAM | 5001 | sign-in, sign-up, logout |
| `/api/v1/users/*` | IoBuild.IAM | 5001 | CRUD usuarios |
| `/api/v1/profiles/*` | IoBuild.IAM | 5001 | Perfiles de usuario |
| `/api/v1/devices/*` | IoBuild.Devices | 5002 | CRUD dispositivos IoT |
| `/api/v1/projects/*` | IoBuild.Projects | 5003 | CRUD proyectos |
| `/api/v1/units/*` | IoBuild.Projects | 5003 | CRUD unidades |
| `/api/v1/clients/*` | IoBuild.Projects | 5003 | CRUD clientes |
| `/api/v1/subscriptions/*` | IoBuild.Subscriptions | 5004 | CRUD suscripciones |
| `/api/v1/plans/*` | IoBuild.Subscriptions | 5004 | Catálogo de planes |
| `/api/v1/analytics/*` | IoBuild.Analytics | 5005 | Dashboards y métricas |

---

## Cómo funciona internamente (YARP)

El Gateway usa **YARP Reverse Proxy** configurado vía `appsettings.json`. Define dos conceptos clave:

### Routes (Rutas)
Cada ruta es una regla que dice "si el path coincide, redirige al cluster X":

```json
"iam-auth": {
  "ClusterId": "iam-cluster",
  "Match": { "Path": "/api/v1/authentication/{**catch-all}" }
}
```

### Clusters (Destinos)
Cada cluster define a dónde enviar las peticiones, con su política de balanceo y health check:

```json
"iam-cluster": {
  "LoadBalancingPolicy": "RoundRobin",
  "Destinations": {
    "iam-destination": { "Address": "http://localhost:5001/" }
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

### Pipeline completo

```
Request entrante
  │
  ▼
┌─────────────────────────┐
│ GlobalExceptionHandler   │ ← Captura errores globales
└─────────────────────────┘
  │
  ▼
┌─────────────────────────┐
│ CORS Policy              │ ← Permite/deniega orígenes
└─────────────────────────┘
  │
  ▼
┌─────────────────────────┐
│ Health Checks (/health) │ ← Monitorea estado de servicios
└─────────────────────────┘
  │
  ▼
┌─────────────────────────┐
│ YARP Reverse Proxy       │ ← Enruta al microservicio
└─────────────────────────┘
  │
  ▼
Microservicio destino (:5001-5005)
  │
  ▼
┌─────────────────────────┐
│ GlobalExceptionHandler   │ ← Captura errores del servicio
└─────────────────────────┘
  │
  ▼
┌─────────────────────────┐
│ RequestAuthorization     │ ← Valida JWT (solo en IAM)
└─────────────────────────┘
  │
  ▼
Controller → Service → Repository → MySQL
```

---

## Health Checks

El Gateway monitorea activamente la salud de los 5 microservicios cada **10 segundos**.

```bash
curl http://localhost:8080/health
```

**Respuesta exitosa (todo Healthy):**
```json
{
  "status": "Healthy",
  "summary": "5 services checked",
  "services": {
    "IoBuild.IAM":          { "status": "Healthy" },
    "IoBuild.Devices":      { "status": "Healthy" },
    "IoBuild.Projects":     { "status": "Healthy" },
    "IoBuild.Subscriptions": { "status": "Healthy" },
    "IoBuild.Analytics":    { "status": "Healthy" }
  }
}
```

**Respuesta con servicio caído:**
```json
{
  "status": "Unhealthy",
  "services": {
    "IoBuild.IAM": { "status": "Unhealthy" },
    "...": "..."
  }
}
```

Cada microservicio también expone su propio `/health` individual:

```bash
curl http://localhost:5001/health   # IAM
curl http://localhost:5002/health   # Devices
curl http://localhost:5003/health   # Projects
# ...etc
```

---

## Resiliencia y Escalabilidad

| Característica | Configuración |
|---------------|--------------|
| **Balanceo** | RoundRobin (distribuye equitativamente entre réplicas) |
| **Health Check activo** | Cada 10s, timeout 5s, política `ConsecutiveFailures` |
| **Timeouts** | Conectados a servicios con timeout de 5s por health check |
| **Escalado horizontal** | Agregar más destinos al cluster = más réplicas |

---

## Opciones de Gateway

| Opción | Ventajas | Desventajas |
|--------|----------|------------|
| **YARP** (usado) | Nativo .NET, configuración JSON, liviano | Menos features que alternativas maduras |
| **Ocelot** | Gateway maduro, agregación de respuestas | Más complejo de configurar |
| **Kong / Nginx** | Extremadamente maduro, plugins, rate limiting | No es .NET, requiere infraestructura adicional |

---

## Seguridad

El Gateway NO valida JWT (eso lo hace cada microservicio individualmente, específicamente IAM). Pero sí provee:

- **CORS** configurado para `http://localhost:5173` (Vue dev server)
- **GlobalExceptionHandler** que evita que errores internos se filtren al cliente
- **Health checks** que permiten detectar servicios caídos

---

## ¿Qué pasa si un microservicio se cae?

Los demás siguen funcionando. Ejemplo:

```
Si IoBuild.Devices (:5002) se cae:
  ✅ /api/v1/authentication/sign-in  → funciona (IAM está vivo)
  ✅ /api/v1/projects                 → funciona (Projects está vivo)
  ❌ /api/v1/devices                  → 502 Bad Gateway
  ⚠️ GET /health → "Unhealthy" con Devices en rojo
```

Esto se conoce como **aislamiento de fallos** — un beneficio clave de los microservicios.
