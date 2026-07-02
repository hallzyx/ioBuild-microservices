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

## MS-03: MySQL por servicio (database-per-service real)

**Reemplaza:** ADR-03 de la Iteración 1 ("MySQL compartido, misma instancia, diferentes BDs").

### Qué hace

Cada microservicio con persistencia (IAM, Devices, Projects, Subscriptions, Analytics, Profiles) corre su **propio contenedor MySQL 8.0** en vez de compartir una sola instancia con 6 bases de datos separadas. En desarrollo se exponen en puertos `3307-3312` (`docker-compose.override.yml`); en producción cada uno vive en la red interna de Docker sin puerto publicado.

### Por qué se revirtió la decisión original

La Iteración 1 aceptó el riesgo de "una instancia, varias BDs" como compromiso pragmático para un VPS de 2 GB. Una vez que el deploy se movió a una VM de Azure con más memoria disponible, ese compromiso ya no era necesario y el aislamiento real de fallos a nivel de base de datos (si `mysql-devices` se cae, `mysql-iam` sigue sirviendo) pasó a ser alcanzable sin costo relevante.

### Configuración

```yaml
mysql-iam:
  image: mysql:8.0
  container_name: iobuild-mysql-iam
  mem_limit: 256m
# ... un servicio análogo por cada microservicio (mysql-devices, mysql-projects,
# mysql-subscriptions, mysql-analytics, mysql-profiles)
```

Cada microservicio apunta a su propio host vía `DB_HOST=mysql-<servicio>`.

> **Nota:** la blacklist de tokens en Redis (`RedisTokenBlacklistService`) **no** es una migración de soporte — sigue satisfaciendo el driver QA-1 de la Iteración 1 (revocación de tokens), solo que con una táctica distinta. Está documentada como evolución de QA-1 en [`docs/iterations/iteration-1-base-seguridad.md`](../iterations/iteration-1-base-seguridad.md), no acá.

---

## MS-04: RabbitMQ como bus de eventos de dominio (Outbox distribuido)

**Contexto:** la Iteración 3 introdujo el Transactional Outbox Pattern para satisfacer QA-3 (consistencia pago↔suscripción) y explícitamente descartó RabbitMQ para ese caso puntual ("Overkill para la escala actual, el `OutboxWorker` en el mismo proceso es suficiente"). Esa parte del Outbox (Subscriptions, evento `subscription.activated`) sigue siendo responsabilidad de QA-3 y no es lo que documenta esta migración.

### Qué hace

`RabbitMqDomainEventPublisher` (`src/IoBuild.Shared/Infrastructure/Messaging/`) publica eventos de dominio al exchange topic `iobuild.domain.events` en RabbitMQ. El patrón Outbox se replicó, con RabbitMQ como transporte, a **IAM, Devices y Projects** — cada uno con su propio `OutboxWorker` + tabla `OutboxMessage` — para casos de uso que ninguna de las 3 iteraciones ADD cubre: provisión de dispositivos por piso (`FloorProvisioningConsumer`), vinculación de dueño-unidad (`OwnerLinkingConsumer`, `UnitOwnerProjectionConsumer`) y anuncios de propietario (`UnitOwnerAnnouncer`).

### Por qué es una migración de soporte (no una reversión de QA-3)

QA-3 solo exige consistencia entre el pago y la suscripción — un evento, un consumidor, in-process alcanza y sigue alcanzando. Lo que forzó a introducir un broker real fue la necesidad de comunicación asíncrona entre **varios** bounded contexts (Devices ⇄ Projects ⇄ Analytics) para features nuevas (aprovisionamiento de dispositivos por piso, vinculación de dueños) que no estaban en el backlog de ninguna de las 3 iteraciones. RabbitMQ resultó ser la pieza de infraestructura compartida que ya existía y que ambos casos (pagos y eventos de dominio) terminaron reutilizando.

### Configuración

```yaml
rabbitmq:
  image: rabbitmq:4-management
  container_name: iobuild-rabbitmq
  healthcheck:
    test: ["CMD", "rabbitmq-diagnostics", "-q", "ping"]
```

```csharp
private const string ExchangeName = "iobuild.domain.events";
private const string ExchangeType = "topic";
```

---

## Resumen

| ID | Tipo | Descripción | Estado |
|----|------|-------------|--------|
| **MS-01** | Nuevo microservicio | `IoBuild.Profiles` — puerto 5006, BD `iobuild_profiles` | ✅ Implementado |
| **MS-02** | Routing Gateway | `/api/v1/webhooks/*` → `subscriptions-cluster` | ✅ Implementado |
| **MS-03** | Infraestructura | MySQL por servicio (6 contenedores en vez de 1 instancia compartida) — decisión de ops no cubierta por CON-1 | ✅ Implementado |
| **MS-04** | Infraestructura | RabbitMQ como bus de eventos de dominio para IAM/Devices/Projects (aprovisionamiento de dispositivos, vinculación de dueños) — no cubierto por QA-3 | ✅ Implementado |

> **Nota:** OpenTelemetry + Jaeger (tracing distribuido) **ya no aparece acá** — se formalizó como driver propio (**QA-4**) en la [Iteración 4](../iterations/iteration-4-observabilidad.md), así que dejó de ser una migración "fuera de alcance de las 3 iteraciones ADD". Tampoco aparece la blacklist de tokens en Redis, que es una evolución del driver QA-1 (Iteración 1) documentada en [`iteration-1-base-seguridad.md`](../iterations/iteration-1-base-seguridad.md).
