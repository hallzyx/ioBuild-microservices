# Iteration 1 — Cierre Formal (QA Gate)

## Matriz de Cobertura Final

| Driver | Estado Anterior | Estado Final | Evidencia |
|--------|----------------|--------------|-----------|
| **CRN-1** (Greenfield) | Addressed | **✅ Complete** | `IoBuild.sln` con 7 proyectos: IoBuild.Shared, IAM, Devices, Projects, Subscriptions, Analytics, Gateway |
| **QA-1** (Seguridad) | Partially Addressed | **✅ Complete** | JWT HMAC-SHA256, BCrypt, Middleware de Autorización con blacklist, Endpoint `/logout`, `ITokenBlacklistService` con `IMemoryCache` |
| **CON-1** (Microservicios) | Addressed | **✅ Complete** | 5 microservicios independientes (5001-5005) + API Gateway YARP en puerto 8080 |
| **CON-2** (Vue Frontend) | Addressed | **✅ Complete** | CORS configurado para `localhost:5173`, Kebab-case routing en todos los servicios |

## Riesgos Mitigados

| Riesgo Original | Estrategia de Mitigación | Estado |
|----------------|-------------------------|--------|
| API Gateway como *Single Point of Failure* | Health checks activos cada 10s con política `ConsecutiveFailures`, RoundRobin load balancing preparado para múltiples réplicas | Mitigado |
| JWT comprometido sin revocación | `ITokenBlacklistService` con `IMemoryCache`, endpoint `POST /authentication/logout`, middleware valida blacklist en cada request | Mitigado |

## Arquitectura Final — Iteración 1

```
Cliente (Vue Web App :5173)
       │
       ▼
IoBuild.Gateway (:8080) ──── Health Checks (/health)
  │       │       │        │          │
  │       │       │        │          ├─ IoBuild.IAM (:5001)
  │       │       │        │          ├─ IoBuild.Devices (:5002)
  │       │       │        │          ├─ IoBuild.Projects (:5003)
  │       │       │        │          ├─ IoBuild.Subscriptions (:5004)
  │       │       │        │          └─ IoBuild.Analytics (:5005)
  ▼       ▼       ▼        ▼
/auth  /devices  /projects /subscriptions
/users  /devices/{id}  /units  /plans      /analytics
/profiles              /clients  /subscriptions/payments

Compartido: IoBuild.Shared.dll
  ├── IBaseRepository<T>, IUnitOfWork
  ├── GlobalExceptionHandlerMiddleware
  ├── KebabCaseRouteNamingConvention
  ├── ModelBuilderExtensions (SnakeCase)
  ├── ITokenBlacklistService (Revocación JWT)
  └── IEvent
```

## ADRs Cerrados

| ADR | Decisión | Estado |
|-----|----------|--------|
| **ADR-01** | Adoptar API Gateway con YARP | **Implementado** — `IoBuild.Gateway` con YARP Reverse Proxy, health checks activos |
| **ADR-02** | Separar IAM como Microservicio | **Implementado** — `IoBuild.IAM` con auth completa + logout + blacklist |
