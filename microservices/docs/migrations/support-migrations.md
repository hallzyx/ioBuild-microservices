# Migraciones de Soporte

> Las **migraciones de soporte** son cambios al sistema que no pertenecen al scope de ninguna de las tres iteraciones ADD pero que fueron necesarios para mantener la coherencia y el funcionamiento del sistema.

---

## ¿Qué es una migración de soporte?

El ADD (Attribute-Driven Design) define iteraciones con drivers claros (QA, US, CON, CRN). Algunos cambios son infraestructura de soporte: agregan bounded contexts nuevos, corrigen rutas del gateway, o ajustan la configuración, pero no responden a un driver ADD específico de las iteraciones 1, 2 o 3.

---

## MS-01: Microservicio IoBuild.Profiles

**Puerto:** `5006`  
**Base de datos:** `iobuild_profiles`  
**Cluster Gateway:** `profiles-cluster`

### Qué hace

Gestiona los perfiles de usuario de IoBuild. Un "perfil" es la información personal del usuario (nombre completo, teléfono, email secundario) separada de su identidad de acceso (IAM).

**Endpoints:**

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| `POST` | `/api/v1/profiles` | ✅ JWT | Crear perfil |
| `GET` | `/api/v1/profiles` | ✅ JWT | Listar perfiles (o filtrar por `?userId=`) |
| `GET` | `/api/v1/profiles/{id}` | ✅ JWT | Obtener perfil por ID |
| `PUT` | `/api/v1/profiles/{id}` | ✅ JWT | Actualizar perfil |
| `POST` | `/api/v1/profiles/second-email` | ✅ JWT | Asignar email secundario (`?userId=`) |

### Arquitectura interna

Sigue la misma Clean Architecture de 4 capas que los demás microservicios:

```
IoBuild.Profiles/
├── Domain/
│   ├── Model/Aggregates/Profile.cs
│   ├── Model/Commands/  (Create, Update)
│   ├── Model/Queries/   (GetById, GetByUserId, GetAll)
│   ├── Repositories/IProfileRepository.cs
│   └── Services/IProfileCommandService.cs + IProfileQueryService.cs
├── Application/
│   ├── Internal/CommandServices/ProfileCommandService.cs
│   ├── Internal/QueryServices/ProfileQueryService.cs
│   └── ACL/ProfilesContextFacade.cs
├── Infrastructure/
│   └── Persistence/EFC/
│       ├── ProfilesDbContext.cs
│       ├── Repositories/ProfileRepository.cs
│       └── Configuration/Seed/ProfilesSeedData.cs
└── Interfaces/
    ├── ACL/IProfilesContextFacade.cs
    └── REST/ProfilesController.cs
```

### Anti-Corruption Layer (ACL)

`IProfilesContextFacade` expone operaciones del Bounded Context de Profiles hacia otros servicios sin acoplar directamente sus dominios. Otros microservicios que necesiten datos de perfil usan esta interfaz, no acceden directamente al DbContext de Profiles.

### ¿Por qué está separado de IAM?

IAM gestiona **identidad y acceso** (credenciales, roles, JWT). El perfil es **información de presentación** (nombre, contacto). Mezclarlos en IAM viola el principio de responsabilidad única: si mañana quisiéramos enriquecer los perfiles con fotos, preferencias o historial de pagos, no tendría sentido hacerlo dentro del servicio de autenticación.

### Configuración en el sistema

**docker-compose.yml:**
```yaml
profiles:
  build:
    context: .
    dockerfile: src/IoBuild.Profiles/Dockerfile
  container_name: iobuild-profiles
  environment:
    - DB_NAME=iobuild_profiles
```

**docker-compose.prod.yml:**
```yaml
profiles:
  image: ghcr.io/hallzyx/iobuild-profiles:${IMAGE_TAG:-latest}
  container_name: iobuild-profiles
```

**Gateway appsettings.json:**
```json
"iam-profiles": {
  "ClusterId": "profiles-cluster",
  "Match": { "Path": "/api/v1/profiles/{**catch-all}" }
},
"profiles-cluster": {
  "Destinations": {
    "profiles-destination": { "Address": "http://localhost:5006/" }
  }
}
```

---

## MS-02: Ruta de Webhooks en el Gateway

**Ruta:** `/api/v1/webhooks/*` → `IoBuild.Subscriptions (:5004)`

### Qué hace

Expone un endpoint dedicado para que Stripe notifique el resultado de los pagos (webhook). Está en un path separado de `/api/v1/subscriptions/` para:

1. Hacer explícita la URL que se configura en el dashboard de Stripe
2. Permitir aplicar middleware específico de verificación de firma solo en este path
3. Separar el tráfico externo (Stripe) del tráfico interno (frontend)

### Configuración en Gateway

```json
"webhooks-all": {
  "ClusterId": "subscriptions-cluster",
  "Match": { "Path": "/api/v1/webhooks/{**catch-all}" }
}
```

El cluster ya existe (`subscriptions-cluster` apunta a `:5004`), así que la migración solo agrega la regla de routing adicional.

---

## Resumen

| ID | Tipo | Descripción | Estado |
|----|------|-------------|--------|
| **MS-01** | Nuevo microservicio | `IoBuild.Profiles` — puerto 5006, BD `iobuild_profiles` | ✅ Implementado |
| **MS-02** | Routing Gateway | `/api/v1/webhooks/*` → `subscriptions-cluster` | ✅ Implementado |
