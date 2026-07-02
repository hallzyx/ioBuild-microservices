# IoBuild — Reporte Final de la Iteración 4

## Observabilidad y Trazabilidad Distribuida (OpenTelemetry + Jaeger)

**Proyecto:** IoBuild — Sistema de Gestión de Propiedades e IoT
**Curso:** Fundamentos de Arquitectura de Software — UPC
**Iteración:** 4
**Estado:** Implementado ✅

---

## Índice

1. [Objetivo de la Iteración](#1-objetivo-de-la-iteración)
2. [Drivers Arquitectónicos](#2-drivers-arquitectónicos)
3. [Decisiones Arquitectónicas (ADRs)](#3-decisiones-arquitectónicas-adrs)
4. [Componentes Implementados](#4-componentes-implementados)
5. [Cobertura en el Proyecto](#5-cobertura-en-el-proyecto)
6. [Cómo Leer un Trace](#6-cómo-leer-un-trace)
7. [Qué NO cubre esta iteración](#7-qué-no-cubre-esta-iteración)
8. [Evolución desde Iteración 3](#8-evolución-desde-iteración-3)

---

## 1. Objetivo de la Iteración

Con 7 microservicios .NET comunicándose por HTTP (Gateway → servicio) y por eventos (RabbitMQ, ver [Iteración 5](iteration-5-eventos-dominio.md)), una sola request de usuario puede cruzar varios procesos y contenedores. Sin tracing distribuido, diagnosticar una request lenta o un error intermitente significaba correlacionar logs de varios contenedores a mano — lento y propenso a error.

**El objetivo de esta iteración:**
> *"Instrumentar el sistema con tracing distribuido para que cualquier request cruzando Gateway → microservicio(s) → infraestructura sea reconstruible como un único trace end-to-end, sin agregar lógica de negocio nueva."*

---

## 2. Drivers Arquitectónicos

| ID | Tipo | Descripción | Cobertura |
|:--:|:----:|------------|:---------:|
| **QA-4** | Quality Attribute | **Observabilidad / Diagnosticabilidad:** ante una request que cruza 2+ microservicios, un operador reconstruye el trace completo sin acceder a logs de contenedores individuales. | ✅ OpenTelemetry + Jaeger, 7/7 servicios .NET instrumentados |
| **CRN-2** | Architectural Concern | Instrumentación transversal — agregar tracing a un microservicio nuevo no debe requerir código repetido. | ✅ `AddIoBuildObservability()` en `IoBuild.Shared`, una línea por `Program.cs` |
| **CON-1** | Constraint | El backend debe implementarse bajo un enfoque de microservicios; la observabilidad no debe acoplar servicios entre sí. | ✅ Cada servicio exporta sus propios spans de forma independiente |

---

## 3. Decisiones Arquitectónicas (ADRs)

### ADR-13: OpenTelemetry con auto-instrumentación (ASP.NET Core + HttpClient)

**Decisión:** Usar el SDK de OpenTelemetry con instrumentación automática de ASP.NET Core (requests entrantes) y HttpClient (requests salientes, ej. Gateway → microservicio), en vez de instrumentación manual por endpoint.

**Racional:** Con auto-instrumentación, cada endpoint HTTP queda cubierto sin tocar el código de negocio. Instrumentar manualmente 7 servicios endpoint por endpoint no escala y viola CRN-2.

**Trade-off:** No captura automáticamente spans de lógica interna (ej. una query EF Core específica o un ciclo del `OutboxWorker`) — solo el borde HTTP entrante/saliente. Se aceptó como límite conocido; agregar `ActivitySource` manual queda como trabajo futuro si hiciera falta más granularidad.

### ADR-14: Jaeger All-in-One sin almacenamiento persistente

**Decisión:** Usar la imagen `jaegertracing/all-in-one` con su backend en memoria por defecto, sin volumen ni base de datos externa (Elasticsearch/Cassandra).

**Racional:** El objetivo es diagnosticar en el momento, no auditar históricamente. Un backend productivo de Jaeger es sobre-ingeniería para el volumen de tráfico de un proyecto académico.

**Trade-off:** Las trazas se pierden al reiniciar el contenedor `iobuild-jaeger` (no hay volumen declarado en `docker-compose.yml` / `docker-compose.prod.yml`). No apto para retención a largo plazo ni compliance.

### Conceptos Descartados

| Concepto | Motivo |
|----------|--------|
| **Métricas (Prometheus + Grafana)** | Fuera del alcance de QA-4, que pide diagnosticar requests, no dashboards de infraestructura. Sin dependencia en `docker-compose.yml`. |
| **Logs centralizados (Loki / ELK)** | Mismo motivo — tercer pilar de observabilidad no cubierto por esta iteración. |
| **Tracing manual con `Activity` por controlador** | No escala con CRN-2 (instrumentación transversal); reemplazado por auto-instrumentación vía SDK. |

---

## 4. Componentes Implementados

### 4.1 `ObservabilityExtensions` (Shared)

**Ubicación:** `src/IoBuild.Shared/Infrastructure/Observability/ObservabilityExtensions.cs`

```csharp
public static IServiceCollection AddIoBuildObservability(
    this IServiceCollection services,
    string serviceName)
{
    var otlpEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");

    services.AddOpenTelemetry()
        .ConfigureResource(r => r.AddService(serviceName))
        .WithTracing(tracing =>
        {
            tracing
                .AddAspNetCoreInstrumentation(o => o.RecordException = true)
                .AddHttpClientInstrumentation();

            if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                tracing.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
        });

    return services;
}
```

`RecordException = true` adjunta el stack trace de excepciones no manejadas como evento del span — así un 500 no solo aparece como status code, sino con el error real dentro del trace.

### 4.2 Registro por servicio

Cada microservicio agrega una sola línea en su `Program.cs`:

```csharp
builder.Services.AddIoBuildObservability("IoBuild.<Servicio>");
```

### 4.3 Colector Jaeger

**Configuración (`docker-compose.yml` / `docker-compose.prod.yml`):**

```yaml
jaeger:
  image: jaegertracing/all-in-one:latest
  container_name: iobuild-jaeger
  environment:
    - COLLECTOR_OTLP_ENABLED=true
```

Cada servicio recibe `OTEL_EXPORTER_OTLP_ENDPOINT=http://jaeger:4317` como variable de entorno, apuntando al puerto gRPC del colector OTLP embebido en Jaeger.

---

## 5. Cobertura en el Proyecto

### 5.1 Servicios instrumentados

| Servicio | `Program.cs` | Instrumentado |
|----------|:---:|:---:|
| IoBuild.Gateway | `src/IoBuild.Gateway/Program.cs:62` | ✅ |
| IoBuild.IAM | `src/IoBuild.IAM/Program.cs:119` | ✅ |
| IoBuild.Devices | `src/IoBuild.Devices/Program.cs:126` | ✅ |
| IoBuild.Projects | `src/IoBuild.Projects/Program.cs:115` | ✅ |
| IoBuild.Subscriptions | `src/IoBuild.Subscriptions/Program.cs:89` | ✅ |
| IoBuild.Analytics | `src/IoBuild.Analytics/Program.cs:80` | ✅ |
| IoBuild.Profiles | `src/IoBuild.Profiles/Program.cs:93` | ✅ |
| Frontend (Nginx/Vue) | — | ❌ No emite spans (fuera de alcance) |
| IoT Simulator (Python) | — | ❌ No emite spans (fuera de alcance) |

**Cobertura: 7/7 microservicios .NET (100%).**

### 5.2 Qué se captura por span

| Dato | Capturado |
|------|:---:|
| Ruta HTTP + método | ✅ (`AddAspNetCoreInstrumentation`) |
| Status code de respuesta | ✅ |
| Duración de la request | ✅ |
| Nombre del servicio (`service.name`) | ✅ (`ConfigureResource`) |
| Excepciones no manejadas + stack trace | ✅ (`RecordException = true`) |
| Requests HTTP salientes (Gateway → microservicio, o microservicio → microservicio vía `ContextFacade`) | ✅ (`AddHttpClientInstrumentation`) |
| Spans de negocio custom (ej. duración de un ciclo de `OutboxWorker`, de una query EF Core específica) | ❌ (requeriría `ActivitySource` manual, no implementado) |
| Eventos de RabbitMQ (publish/consume) como parte del mismo trace | ❌ (RabbitMQ.Client no tiene auto-instrumentación de OpenTelemetry configurada; el consumo aparece como un trace nuevo, no correlacionado con el publish original) |
| Métricas (throughput, latencia p95/p99 agregada) | ❌ (pilar de métricas, fuera de alcance — ver ADRs descartados) |
| Logs correlacionados con trace ID | ❌ (pilar de logs, fuera de alcance) |

### 5.3 Acceso

| Recurso | URL |
|---------|-----|
| Jaeger UI (búsqueda de traces) | `https://watcher.arroz.dev` |
| Colector OTLP (interno, red Docker) | `http://jaeger:4317` |

---

## 6. Cómo Leer un Trace

1. Abrir `https://watcher.arroz.dev`.
2. Elegir un servicio en el dropdown (ej. `IoBuild.Gateway`).
3. Buscar por rango de tiempo o por operación (ej. `POST /api/v1/devices`).
4. Abrir un trace: se ve el **waterfall** — cada span como una barra horizontal, anidado por quién llamó a quién (Gateway → Devices, en el caso de una request que cruza el proxy).
5. Un span en rojo indica excepción — al expandirlo se ve el stack trace capturado por `RecordException`.

**Limitación conocida:** si una request dispara un evento asíncrono vía RabbitMQ (ej. crear un dispositivo dispara `DeviceProvisioned` hacia Projects), el trace original **no incluye** el procesamiento del consumidor — aparece como un trace nuevo y separado, porque no hay propagación de contexto (`traceparent`) a través de los headers AMQP. Ver la fila correspondiente en la tabla de cobertura (§5.2).

---

## 7. Qué NO cubre esta iteración

Para que quede explícito y no se lea como "observabilidad completa":

- **Métricas** (Prometheus/Grafana) — no hay dashboards de CPU, memoria, throughput o latencia agregada por servicio.
- **Logs centralizados** — cada servicio sigue logueando a stdout/Docker logs; no hay Loki/ELK correlacionando logs con trace IDs.
- **Propagación de trace context a través de RabbitMQ** — eventos de dominio no continúan el trace HTTP que los originó.
- **Retención de trazas** — Jaeger corre sin almacenamiento persistente (ADR-14); un restart borra el historial.
- **Sampling** — se exporta el 100% de las requests, sin ajuste de tasa de muestreo.

---

## 8. Evolución desde Iteración 3

| Aspecto | Iteración 3 | Iteración 4 |
|---------|:-----------:|:-----------:|
| **Visibilidad de requests cross-servicio** | Solo logs de Docker por contenedor | **Trace end-to-end en Jaeger UI** |
| **Diagnóstico de errores 500** | Buscar en logs manualmente | **Stack trace capturado como evento del span** |
| **Servicios instrumentados** | 0 | **7/7 microservicios .NET** |
| **Nuevo componente de infraestructura** | — | **Jaeger All-in-One** (`iobuild-jaeger`) |
| **Código nuevo por servicio** | — | 1 línea (`AddIoBuildObservability`) en cada `Program.cs` |

---

> **Documento generado para el curso de Fundamentos de Arquitectura de Software — UPC**
> **Proyecto:** IoBuild — Iteración 4 (Observabilidad)
> **Estado:** ✅ Implementado — OpenTelemetry + Jaeger, 7/7 servicios .NET instrumentados
