# IoBuild — Iteration 2 Final Report

## Pipeline de Telemetría IoT: MQTT + InfluxDB + Worker + Simulador

**Proyecto:** IoBuild — Sistema de Gestión de Propiedades e IoT  
**Curso:** Fundamentos de Arquitectura de Software — UPC  
**Iteración:** 2  
**Fecha:** Mayo 2026  
**Estado:** Completado y Verificado ✅  

---

## Índice

1. [Objetivo de la Iteración](#1-objetivo-de-la-iteración)
2. [Drivers Arquitectónicos](#2-drivers-arquitectónicos)
3. [Decisiones Arquitectónicas (ADRs)](#3-decisiones-arquitectónicas-adrs)
4. [Arquitectura del Pipeline](#4-arquitectura-del-pipeline)
5. [Componentes Implementados](#5-componentes-implementados)
6. [Pipeline de Datos Completo](#6-pipeline-de-datos-completo)
7. [Bugs Encontrados y Resueltos](#7-bugs-encontrados-y-resueltos)
8. [Resultados de Testing](#8-resultados-de-testing)
9. [Evolución desde Iteración 1](#9-evolución-desde-iteración-1)
10. [Estadísticas de la Iteración](#10-estadísticas-de-la-iteración)
11. [Arquitectura Final (C4 Container)](#11-arquitectura-final-c4-container)
12. [Próximos Pasos](#12-próximos-pasos)

---

## 1. Objetivo de la Iteración

La Iteración 1 estableció la base arquitectónica: 5 microservicios, API Gateway, seguridad JWT y testing BDD. El sistema funcionaba, pero **no había forma de recibir datos reales de dispositivos IoT**, que es el core del dominio de IoBuild.

**El problema:** El microservicio Devices escribía telemetría en MySQL (la misma base de datos que el inventario). Bajo carga de múltiples dispositivos enviando datos de alta frecuencia, las escrituras compiten con las operaciones CRUD web y degradan la experiencia del usuario. Además, MySQL no está optimizado para queries de series temporales como "consumo de energía por hora".

**El objetivo de esta iteración:** Diseñar e implementar un pipeline de ingesta asíncrona de telemetría IoT que permita recibir datos de múltiples dispositivos sin degradar el rendimiento del sistema web, y exponer endpoints de consulta para visualizar energía y estado en tiempo real.

---

## 2. Drivers Arquitectónicos

| ID | Tipo | Descripción | Cobertura |
|:--:|:----:|------------|:---------:|
| **QA-2** | Quality Attribute | **Escalabilidad / Rendimiento:** Múltiples dispositivos envían telemetría simultáneamente. El sistema procesa los estados sin interrumpir la operación web. | ✅ Pipeline asíncrono (MQTT + Worker) desacopla escritura de lectura. MySQL no se toca para telemetría. |
| **US12** | Primary Functionality | Gráfico de consumo de energía por hora para evaluar el rendimiento energético de los proyectos. | ✅ Endpoint `GET /devices/{id}/energy` retorna datos desde InfluxDB. |
| **US33** | Primary Functionality | Ver la lista de dispositivos registrados y monitorear su estado y ubicación en tiempo real. | ✅ Endpoint `GET /devices/{id}/status` retorna último estado conocido desde InfluxDB. |
| **CRN-3** | Architectural Concern | Separación del almacenamiento de telemetría (alta escritura) del inventario relacional estático. | ✅ Persistencia políglota: MySQL para inventario, InfluxDB para telemetría. Sin superposición. |

---

## 3. Decisiones Arquitectónicas (ADRs)

| ID | Decisión | Racional | Trade-off Aceptado |
|:--:|----------|----------|-------------------|
| **ADR-03** | Uso de **Mosquitto MQTT** como broker de ingesta IoT | MQTT es el protocolo estándar de IoT. Mosquitto pesa ~8 MB RAM y agrega ~2 bytes de overhead por mensaje. Desacopla la alta carga de eventos de las API REST web (QA-2). | Requiere infraestructura adicional (broker MQTT). Mitigado: Mosquitto es liviano (16 MB de límite en Docker). |
| **ADR-04** | **Persistencia Políglota** (MySQL + InfluxDB) | Separar datos transaccionales (inventario) de telemetría temporal evita que escrituras masivas degraden las operaciones CRUD (CRN-3). InfluxDB está optimizado para queries de rango temporal como `GROUP BY time`. | Aumenta la complejidad operativa: 2 bases de datos que respaldar. Mitigado: InfluxDB usa retención automática de 7 días, sin backup necesario para datos temporales. |
| **ADR-05** | **Worker in-process** dentro de IoBuild.Devices (BackgroundService) | El Telemetry Worker comparte el mismo proceso que la API de Devices. Evita un contenedor extra, un Dockerfile adicional y configuración de red. Para MVP, el acoplamiento es aceptable. | Si el worker falla, no afecta la API (son hilos separados). Si la API se cae, el worker también. En producción se puede extraer a un contenedor separado sin cambiar el código del worker. |
| **ADR-06** | **PointData API** (no WriteMeasurement POCO) para escribir en InfluxDB | `WriteMeasurement` con POCO y atributos `[Column]` tuvo problemas de serialización. PointData con `.Field()` explícitos da control total y garantiza que los valores se escriban correctamente. | Más código verbose al escribir. Mitigado: es un solo método de 8 líneas. |
| **ADR-07** | **Simulador en Python Alpine** (no .NET) | El simulador es una herramienta de desarrollo, no un componente arquitectónico. Un script Python de ~80 líneas pesa ~50 MB (vs ~220 MB de una imagen .NET). | Dependencia de Python en el stack. Mitigado: es solo para desarrollo/MVP, no va a producción. |

### Conceptos Descartados

| Concepto | Motivo | Reemplazo |
|----------|--------|-----------|
| **Guardar telemetría en MySQL** (tabla única) | Con miles de eventos por minuto, las operaciones I/O bloquearían la base de datos transaccional y degradarían todo el rendimiento web. | InfluxDB + MQTT |
| **Worker como proyecto .NET separado** | Agrega un contenedor extra (+128 MB RAM, + Dockerfile, + CI/CD) sin beneficio real para la escala actual. | BackgroundService dentro de Devices |
| **WebSockets / SignalR** para push en tiempo real | Alcance demasiado grande para esta iteración. La US33 se cubre con consulta REST por ahora. | Futura iteración |
| **RabbitMQ en vez de Mosquitto** | RabbitMQ es ~150 MB RAM vs Mosquitto ~16 MB. Para telemetría pura, MQTT es el protocolo correcto. RabbitMQ sería adecuado para comunicación inter-servicios (Iteración 3). | Mosquitto |

---

## 4. Arquitectura del Pipeline

```
┌─────────────────────────────────────────────────────────────────────┐
│                        PIPELINE DE TELEMETRÍA IoT                    │
│                                                                      │
│  ┌────────────┐   MQTT QoS 1   ┌────────────┐  PointData  ┌───────┐ │
│  │   IoT      │───────────────▶│  Mosquitto  │────────────▶│InfluxDB│ │
│  │ Simulator  │ telemetry/{id} │   Broker    │  WriteAPI   │  OSS  │ │
│  │ (Python)   │                │  (16 MB)    │             │(64 MB)│ │
│  └────────────┘                └──────┬──────┘             └───┬───┘ │
│                                       │ Subscribe              │     │
│                                  ┌────▼────┐              ┌────▼───┐ │
│                                  │Telemetry│              │Devices │ │
│                                  │ Worker  │              │  API   │ │
│                                  │(.NET BG)│              │REST    │ │
│                                  └─────────┘              └────────┘ │
│                                                                      │
│  ┌────────────┐  ┌────────────┐  ┌────────────┐                     │
│  │  Gateway   │◀─│  Frontend  │◀─│  Usuario   │                     │
│  │  (YARP)    │  │  (Nginx)   │  │  Web       │                     │
│  └────────────┘  └────────────┘  └────────────┘                     │
└─────────────────────────────────────────────────────────────────────┘
```

### Flujo de escritura (asíncrono)

```
Simulador ──MQTT──→ Mosquitto ──Subscribe──→ TelemetryWorker ──Write──→ InfluxDB
    ↑                          ↑                         ↑
  Publica cada 5s         Almacena y             Deserializa JSON,
  5 dispositivos         distribuye eventos      escribe PointData
```

### Flujo de lectura (síncrono)

```
Frontend ──GET /energy──→ Gateway ──→ Devices API ──Flux query──→ InfluxDB
    ↑                         ↑              ↑
  Usuario web             Enruta a       Valida device
  (Vue/Nginx)             Devices:5002   en MySQL, consulta
                                         InfluxDB con Flux
```

---

## 5. Componentes Implementados

### 5.1 Mosquitto MQTT (Broker)

| Propiedad | Valor |
|-----------|-------|
| **Imagen** | `eclipse-mosquitto:2-openssl` |
| **RAM** | 16 MB (límite) |
| **Puerto** | 1883 (interno) |
| **QoS** | 1 (at-least-once delivery) |
| **Persistencia** | Habilitada en `/mosquitto/data/` |
| **Autenticación** | Deshabilitada (red interna Docker) |

```ini
# mosquitto/config/mosquitto.conf
listener 1883
allow_anonymous true
persistence true
persistence_location /mosquitto/data/
log_dest stdout
```

**¿Por qué permitir conexiones anónimas?** Porque Mosquitto corre en la red interna de Docker. Solo el Simulador y el Worker tienen acceso. No hay exposición a Internet. Si en el futuro se necesita autenticación, se agrega en el config sin cambiar los consumidores.

---

### 5.2 Telemetry Worker (BackgroundService .NET)

| Propiedad | Valor |
|-----------|-------|
| **Ubicación** | `IoBuild.Devices/Workers/TelemetryWorker.cs` |
| **Herencia** | `BackgroundService` |
| **MQTT Client** | MQTTnet 5.0.1 |
| **Topic suscrito** | `telemetry/#` (multi-level wildcard) |
| **QoS** | AtLeastOnce |
| **InfluxDB Write** | PointData API con `WriteApi` + `using` para flush inmediato |

**Arquitectura del Worker:**

```
ExecuteAsync()
    │
    └── while (!cancelled)
            │
            ├── ConnectAndSubscribeAsync()
            │   ├── Crear MqttClient
            │   ├── Conectar a Mosquitto
            │   ├── Subscribe a telemetry/# (QoS 1)
            │   └── Esperar mensajes (Delay Infinite)
            │
            └── catch (Exception)
                └── Esperar 10s y reconectar
```

**Manejo de mensajes:**

```
OnMessageReceived
    │
    ├── Deserializar JSON → TelemetryRawPayload
    ├── Validar payload no nulo
    └── Escribir en InfluxDB vía ITelemetryWriteService
         └── PointData
             .Tag("deviceId")
             .Tag("location")
             .Field("energy_kwh")
             .Field("temperature_c")
             .Field("voltage_v")
             .Field("status")
```

**Payload MQTT esperado:**
```json
{
  "deviceId": 1,
  "timestamp": "2026-05-25T12:00:00Z",
  "energy_kwh": 2.14,
  "temperature_c": 30.1,
  "voltage_v": 223.0,
  "status": "online",
  "location": "Sector-A"
}
```

**Tolerancia a fallos:** El Worker reconecta automáticamente si Mosquitto se cae. Bucle `while` con `try-catch` y `Task.Delay(10s)` entre intentos. Si el Worker se cae, la API de Devices sigue funcionando (los datos de InfluxDB siguen disponibles para consulta).

---

### 5.3 InfluxDB OSS 2.7 (Time-Series Database)

| Propiedad | Valor |
|-----------|-------|
| **Imagen** | `influxdb:2.7-alpine` |
| **RAM** | 64 MB (límite) |
| **Bucket** | `iobuild-telemetry` |
| **Organización** | `iobuild` |
| **Retención** | 7 días (configurable vía InfluxDB UI) |
| **Esquema** | Measurement: `telemetry` |

**Tags:**
| Tag | Tipo | Ejemplo |
|-----|------|---------|
| `deviceId` | string (tag) | `"1"` |
| `location` | string (tag) | `"Sector-A"` |

**Fields:**
| Field | Tipo | Ejemplo |
|-------|------|---------|
| `energy_kwh` | float | `2.14` |
| `temperature_c` | float | `30.1` |
| `voltage_v` | float | `223.0` |
| `status` | string | `"online"` |

**Ejemplo de consulta Flux para energía horaria (US12):**
```flux
from(bucket: "iobuild-telemetry")
  |> range(start: -24h)
  |> filter(fn: (r) => r._measurement == "telemetry" and r.deviceId == "1")
  |> aggregateWindow(every: 1h, fn: mean)
  |> yield(name: "mean")
```

---

### 5.4 Devices API — Nuevos Endpoints

#### `GET /api/v1/devices/{id}/energy`

| Propiedad | Valor |
|-----------|-------|
| **Auth** | `[Authorize]` (JWT Bearer) |
| **Query params** | `from` (DateTime?), `to` (DateTime?) — opcionales, default 24h |
| **Response 200** | `[{ "timestamp": "...", "energyKwh": 1.5, "temperatureC": 22.3, "voltageV": 220.1 }]` |
| **Response 200 (sin datos)** | `[]` |
| **Response 404** | `{ "message": "Device with ID X not found" }` |
| **Response 401** | `{ "error": "Authorization token is required." }` |

#### `GET /api/v1/devices/{id}/status`

| Propiedad | Valor |
|-----------|-------|
| **Auth** | `[Authorize]` (JWT Bearer) |
| **Response 200** | `{ "deviceId": 1, "status": "online", "lastSeen": "2026-05-25T12:00:00Z", "temperatureC": 22.3, "voltageV": 220.1 }` |
| **Response 200 (sin datos)** | `{ "deviceId": 1, "status": "unknown", "lastSeen": null, "temperatureC": 0, "voltageV": 0 }` |
| **Response 404** | `{ "message": "Device with ID X not found" }` |
| **Response 401** | `{ "error": "Authorization token is required." }` |

**Ambos endpoints validan que el dispositivo existe en MySQL (inventario) antes de consultar InfluxDB.** Si el device no existe en MySQL, retornan 404 inmediatamente sin tocar InfluxDB.

---

### 5.5 IoT Simulator (Python)

| Propiedad | Valor |
|-----------|-------|
| **Imagen base** | `python:3.12-alpine` ~50 MB |
| **Librería** | `paho-mqtt==2.1.0` |
| **RAM** | 32 MB (límite) |
| **Dispositivos** | 5 (IDs 1-5, locations Sector-A a E) |
| **Frecuencia** | Cada 5 segundos |
| **Payload** | `{ deviceId, timestamp, energy_kwh, temperature_c, voltage_v, status, location }` |
| **Variación** | energy_kwh: 0.5-3.0, temperature_c: 18-35°C, voltage_v: 215-230V |
| **Status** | 75% online, 25% idle |

### 5.6 Frontend: Dashboard Telemetry Display

Para cerrar el círculo y cumplir US12 y US33, se integró la telemetría en el **dashboard de analytics** del frontend. El dashboard existente mostraba datos simulados de Analytics API; ahora **también** consume Devices API para mostrar datos reales del pipeline IoT.

#### Cambios realizados (frontend-only)

| Archivo | Cambio |
|---------|--------|
| `analytics/infrastructure/analytics-api.js` | + `getDeviceEnergy(id, from, to)` y `getDeviceStatus(id)` apuntando a Devices API |
| `analytics/application/analytics.store.js` | + estado `deviceEnergyReadings`, `deviceStatus`, `selectedDeviceId`, `devices` + acciones `fetchDeviceEnergy`, `fetchDeviceStatus`, `selectDevice` |
| `analytics/presentation/components/owner-dashboard.component.vue` | + dropdown de dispositivo + chart energía + status card |
| `analytics/presentation/components/builder-dashboard.component.vue` | + dropdown + chart + status card |
| `locales/en.json`, `locales/es.json` | + 9 keys i18n de telemetría |

#### Flujo de datos

```
Dashboard carga → fetchAnalytics() (stats normales, igual que antes)
                → fetchDevices() (llena dropdown con nombres)
                
Usuario selecciona un dispositivo
                → GET /devices/{id}/energy?from=-24h&to=now
                → GET /devices/{id}/status
                
Chart: Line chart (vue-chartjs) con energyKwh vs timestamp
Status: Badge verde/rojo/gris + lastSeen + temperatura + voltaje
```

#### UI: cómo se ve

```
┌──────────────────────────────────────────────────┐
│  Dashboard de Propiedades                        │
│                                                  │
│  ┌─────────┐ ┌─────────┐ ┌─────────┐            │
│  │ Unidades │ │Disposit.│ │Energía  │            │ ← Stats de Analytics (sin cambios)
│  │    5     │ │   12    │ │ 340 kWh │            │
│  └─────────┘ └─────────┘ └─────────┘            │
│                                                  │
│  ── Telemetría en Vivo ──                        │
│  Dispositivo: [Termostato Lobby ▼]               │ ← Dropdown con dispositivos reales
│                                                  │
│  ┌─────────────────────────┐ ┌───────────────┐  │
│  │ Consumo Energía (kWh)   │ │ Estado        │  │
│  │  ┌──────────────────┐   │ │ 🟢 Online     │  │
│  │  │  ╱╲     ╱╲       │   │ │ Última vez:   │  │
│  │  │ ╱  ╲   ╱  ╲      │   │ │ 12:30         │  │
│  │  │╱    ╲ ╱    ╲     │   │ │ Temp: 22.3°C  │  │
│  │  │      10:00 14:00 │   │ │ Volt: 220V    │  │
│  │  └──────────────────┘   │ └───────────────┘  │
│  └─────────────────────────┘                    │
└──────────────────────────────────────────────────┘
```

#### Decisiones técnicas

| Decisión | Motivo |
|----------|--------|
| Store separado en analytics.store.js | Mantener telemetría separada de CRUD de devices. Cero riesgo de regresión. |
| Consumir Devices API directo (no Analytics API) | Analytics no tiene acceso a InfluxDB. Devices API ya expone los endpoints. |
| Dropdown con lista de dispositivos real | Usa `GET /devices/` existente. No requiere nuevo backend. |
| Chart.js (ya instalado) | Misma librería que los charts existentes de analytics. Sin nuevas dependencias. |
| Polling cada 30s no implementado (v1) | El status se consulta una vez al seleccionar dispositivo. Suficiente para MVP. |
| Sin vista detalle separada | El chart y status se integraron dentro del dashboard existente. Menos fricción de navegación. |

#### Cobertura de User Stories

| User Story | Estado | Verificación |
|:----------:|:------:|-------------|
| **US12** — Gráfico de consumo de energía por hora | ✅ | Dashboard → dropdown → Line chart con datos de InfluxDB |
| **US33** — Estado y ubicación de dispositivos en tiempo real | ✅ | Dashboard → dropdown → status card con badge + lastSeen + métricas |

```python
# Simulador — lógica principal
while True:
    for device_id in range(1, 6):
        payload = {
            "deviceId": device_id,
            "timestamp": datetime.now(timezone.utc).isoformat(),
            "energy_kwh": round(random.uniform(0.5, 3.0), 2),
            "temperature_c": round(random.uniform(18.0, 35.0), 1),
            "voltage_v": round(random.uniform(215.0, 230.0), 1),
            "status": random.choice(["online", "online", "online", "idle"]),
            "location": LOCATIONS[device_id - 1],
        }
        client.publish(f"telemetry/{device_id}", json.dumps(payload), qos=1)
    time.sleep(5)
```

---

## 6. Pipeline de Datos Completo

### Verificación punto a punto

```
Paso 1: Simulador → Mosquitto
────────────────────────────────
$ docker compose logs simulator | tail -3
→ Published to telemetry/1: energy=2.03kWh, temp=23.6C, voltage=225.5V

Paso 2: Mosquitto → Worker
────────────────────────────────
$ docker compose logs devices | grep "Received MQTT"
→ Received MQTT message on telemetry/1

Paso 3: Worker → InfluxDB
────────────────────────────────
$ docker compose logs devices | grep "Written telemetry"
→ Written telemetry for device 1

Paso 4: InfluxDB — consulta directa
────────────────────────────────
$ curl -X POST http://influxdb:8086/api/v2/query?org=iobuild \
  -H "Authorization: Token iobuild-telemetry-token" \
  -d 'from(bucket:"iobuild-telemetry") |> range(start:-5m)'
→ ... energy_kwh=1.58, temperature_c=19.0, status="online" ...

Paso 5: API → InfluxDB (lectura)
────────────────────────────────
$ curl http://localhost:8080/api/v1/devices/1/energy \
  -H "Authorization: Bearer $TOKEN"
→ [{"timestamp":"...","energyKwh":1.58,"temperatureC":19.0,...}]

Paso 6: API → último estado
────────────────────────────────
$ curl http://localhost:8080/api/v1/devices/1/status \
  -H "Authorization: Bearer $TOKEN"
→ {"deviceId":1,"status":"online","lastSeen":"2026-05-25T23:44:32Z","temperatureC":29.4,"voltageV":228.2}
```

---

## 7. Bugs Encontrados y Resueltos

| # | Bug | Causa Raíz | Solución | Impacto |
|:-:|-----|-----------|----------|---------|
| 1 | **energyKwh siempre 0** en InfluxDB | `System.Text.Json` case-sensitive: el record usaba `energyKwh` pero el JSON tiene `energy_kwh`. El campo se deserializaba como 0.0 (default de double). | Renombrar parámetro del record a `energy_kwh` para que coincida exactamente con el JSON. | 🔴 Crítico — los datos de energía no se almacenaban correctamente |
| 2 | **Worker no escribía en InfluxDB** | `WriteApiAsync` usa WriteApiAsync sin `await using`. Los datos quedaban bufferizados y nunca se flusheaban. | Cambiar a `WriteApi` (síncrono) con `using` para forzar flush al dispose. | 🟡 Alto — datos no persistían |
| 3 | **InfluxDB sin datos visibles** | `WriteMeasurementAsync` con POCO y atributos `[Column]` tiene problemas de serialización en InfluxDB.Client 4.x. | Cambiar a `PointData` API con `.Field()` explícitos. | 🔴 Crítico — el pipeline entero no funcionaba |
| 4 | **Worker no recibía mensajes MQTT** | El contenedor devices se levantó antes de que existiera el código del worker. Los cambios no se reflejaban en la imagen Docker. | Forzar `docker compose build --no-cache devices` para reconstruir la imagen. | 🟡 Alto — solo ocurría en desarrollo con cambios de código |
| 5 | **Simulador publicaba en topic incorrecto** | El tópico era `telemetry/device/{id}` en vez de `telemetry/{id}` (inconsistencia con la spec). | Cambiar el topic en `simulator.py` de `telemetry/device/{id}` a `telemetry/{id}`. | 🟢 Bajo — igual funcionaba por `#` wildcard |

---

## 8. Resultados de Testing

### Tests Realizados

| Tipo | Cantidad | Resultado |
|:----:|:--------:|:---------:|
| Unit Tests (nuevos) | 22 | ✅ 22/22 |
| BDD Scenarios (nuevos) | 4 | ✅ 4/4 |
| BDD Scenarios (existentes) | 12 | ✅ 12/12 |
| Integration Tests (existentes) | 10 | ✅ 10/10 |
| **Total** | **48** | **48/48 (100%)** ⭐ |

### Cobertura de Escenarios BDD de Telemetría

```gherkin
Feature: Device Telemetry
  Como un Property Manager
  Quiero consultar la telemetria de mis dispositivos IoT
  Para monitorear el consumo de energia y el estado en tiempo real

  @US12
  Scenario: Consultar consumo de energia por hora
    Given el usuario esta autenticado como "PropertyManager"
    And existe un dispositivo con ID 1
    And el dispositivo tiene datos de telemetria
    When envia GET a "/api/v1/devices/1/energy"
    Then respuesta 200 OK con lista de puntos de energia

  @US33
  Scenario: Consultar ultimo estado conocido
    Given el usuario esta autenticado
    And existe un dispositivo con ID 1
    And el dispositivo tiene telemetria con estado "online"
    When envia GET a "/api/v1/devices/1/status"
    Then respuesta 200 OK con estado y lastSeen

  Scenario: Sin datos de telemetria
    Given el usuario esta autenticado
    And existe un dispositivo con ID 1
    And el dispositivo NO tiene datos de telemetria
    When envia GET a "/api/v1/devices/1/energy"
    Then respuesta 200 OK con lista vacia

  @Security
  Scenario: Usuario no autenticado
    Given el usuario NO esta autenticado
    When envia GET a "/api/v1/devices/1/energy"
    Then respuesta 401 Unauthorized
```

### Verificación Manual en Runtime

| Test | Resultado |
|:----:|:---------:|
| Health Check Gateway | ✅ 200 |
| Energy endpoint (datos presentes) | ✅ 200 con `energyKwh` > 0 |
| Status endpoint (último estado) | ✅ 200 con `status: "online"` |
| Sin token JWT | ✅ 401 |
| Dispositivo inexistente | ✅ 404 |
| Simulador publicando | ✅ 5 dispositivos cada 5s |
| Worker escribiendo en InfluxDB | ✅ 60+ registros en 2 minutos |
| InfluxDB consultable | ✅ Queries Flux retornan datos |

---

## 9. Evolución desde Iteración 1

| Aspecto | Iteración 1 | Iteración 2 |
|---------|:-----------:|:-----------:|
| **Contenedores** | 8 | **11** (+mosquitto, +influxdb, +simulator) |
| **Bases de datos** | 1 (MySQL) | **2** (MySQL + InfluxDB) — políglota |
| **Protocolos** | HTTP/REST | HTTP + **MQTT** (asíncrono) |
| **Canales de entrada** | Web UI (síncrono) | Web UI + **IoT Devices** (asíncrono) |
| **Tests** | 26 (16 BDD + 10 integration) | **48** (+22 unit tests de telemetría) |
| **Lenguajes** | C# (.NET 9) | C# + **Python** (simulador) |
| **Arquitectura** | Monolito → Microservicios | Microservicios + **Event-Driven** |

### Cambios en IoBuild.Devices

| Archivo | Estado | Líneas |
|---------|:------:|:------:|
| `Workers/TelemetryWorker.cs` | ✅ Nuevo | 128 |
| `Infrastructure/InfluxDB/*` (4 archivos) | ✅ Nuevos | ~60 |
| `Infrastructure/Mqtt/MqttOptions.cs` | ✅ Nuevo | 15 |
| `Domain/Model/Aggregates/*` (2 archivos) | ✅ Nuevos | ~10 |
| `Domain/Model/Queries/*` (2 archivos) | ✅ Nuevos | ~8 |
| `Domain/Services/ITelemetryQueryService.cs` | ✅ Nuevo | 12 |
| `Application/Internal/QueryServices/TelemetryQueryService.cs` | ✅ Nuevo | 60 |
| `Interfaces/REST/TelemetryController.cs` | ✅ Nuevo | 62 |
| `Interfaces/REST/Resources/*` (2 archivos) | ✅ Nuevos | ~6 |
| `Interfaces/REST/Transform/TelemetryResourceAssembler.cs` | ✅ Nuevo | 12 |
| `IoBuild.Devices.csproj` | ✏️ Modificado | +2 NuGet packages |
| `appsettings.json` | ✏️ Modificado | +2 secciones |
| `Program.cs` | ✏️ Modificado | +15 líneas DI |
| Tests (6 archivos) | ✅ Nuevos | ~250 |

### Cambios en Frontend (analytics dashboard)

| Archivo | Estado | Líneas |
|---------|:------:|:------:|
| `analytics/infrastructure/analytics-api.js` | ✏️ Modificado | +35 |
| `analytics/application/analytics.store.js` | ✏️ Modificado | +53 |
| `analytics/components/owner-dashboard.component.vue` | ✏️ Modificado | +233 |
| `analytics/components/builder-dashboard.component.vue` | ✏️ Modificado | +233 |
| `locales/en.json`, `es.json` | ✏️ Modificado | +11 c/u |

## 10. Estadísticas de la Iteración

| Métrica | Valor |
|:-------:|:-----:|
| **Archivos nuevos** | 30 (24 backend + 6 frontend) |
| **Archivos modificados** | 11 (6 backend + 5 frontend) |
| **Líneas de código nuevas** | ~1,700 (1,125 backend + 574 frontend) |
| **Líneas eliminadas** | ~61 (stubs UnitTest1.cs) |
| **Commits** | 4 |
| **Contenedores nuevos** | 3 (mosquitto, influxdb, simulator) |
| **Tests nuevos** | 22 unit + 4 BDD |
| **Tests totales** | 48 (100% pasando) |
| **Patrones nuevos** | Event-Driven Architecture, Message Broker, CQRS parcial, Persistencia Políglota |
| **ADRs nuevos** | 5 (ADR-03 a ADR-07) |
| **Bugs resueltos** | 5 |
| **User Stories cubiertas** | US12 (energía), US33 (estado) |
| **Horas estimadas** | ~20-25 horas |

---

## 11. Arquitectura Final (C4 Container)

```mermaid
C4Container
    title IoBuild — Iteración 2: Container Diagram

    Person(web_user, "Property Manager")
    System_Ext(stripe, "Stripe")
    System_Ext(traefik, "Traefik (Dokploy)")

    Container_Boundary(frontend, "Capa de Presentación") {
        Container(nginx, "Frontend (Nginx)", "nginx:alpine", "SPA Vue 3 + proxy inverso")
    }

    Container_Boundary(gateway, "API Gateway") {
        Container(yarp, "Gateway (YARP)", "ASP.NET 9", "Enrutamiento, health checks")
    }

    Container_Boundary(iam_bc, "IAM (Identidad)") {
        Container(iam, "IoBuild.IAM", ":5001", "JWT, login, registro, blacklist")
        ContainerDb(iam_db, "MySQL", "Usuarios y roles")
    }

    Container_Boundary(devices_bc, "Device Management") {
        Container(devices, "IoBuild.Devices", ":5002", "CRUD + telemetría")
        ContainerDb(devices_db, "MySQL", "Inventario de dispositivos")
        Container(worker, "Telemetry Worker", "BackgroundService", "MQTT → InfluxDB")
        ContainerDb(influx, "InfluxDB", "TSDB - telemetría (7 días)")
        ContainerQueue(mqtt, "Mosquitto", "MQTT Broker")
        Container(sim, "IoT Simulator", "Python", "Telemetría fake (dev only)")
    }

    Container_Boundary(others, "Otros BCs") {
        Container(projects, "IoBuild.Projects", ":5003")
        Container(subscriptions, "IoBuild.Subscriptions", ":5004")
        Container(analytics, "IoBuild.Analytics", ":5005")
    }

    Rel(web_user, traefik, "HTTPS")
    Rel(traefik, nginx, "HTTP")
    Rel(nginx, yarp, "/api/*")
    Rel(yarp, iam, "/auth/*, /users/*")
    Rel(yarp, devices, "/devices/*")
    Rel(yarp, projects, "/projects/*")
    Rel(yarp, subscriptions, "/subscriptions/*")
    Rel(yarp, analytics, "/analytics/*")

    Rel(sim, mqtt, "MQTT publish")
    Rel(mqtt, worker, "MQTT subscribe")
    Rel(worker, influx, "PointData write")
    Rel(devices, influx, "Energy/Status read")
    Rel(devices, devices_db, "CRUD")

    UpdateLayoutConfig($c4ShapeInRow="3", $c4BoundaryInRow="3")
```

---

## 12. Próximos Pasos

### Pendientes de esta iteración

| Tarea | Prioridad |
|:----:|:---------:|
| Agregar WebSockets/SignalR para push de estados en tiempo real a la UI | 🟡 Media |
| Extraer TelemetryWorker a contenedor separado si el rendimiento lo requiere | 🟢 Baja |
| Agregar más dispositivos al simulador (configurable vía env var) | 🟢 Baja |
| Pruebas de carga con 100+ dispositivos simulados | 🟡 Media |

### Iteración 3 (según ADD)

La Iteración 3 se enfoca en **Suscripciones, Planes y Pagos Seguros**:

| Driver | Descripción |
|:------:|------------|
| **QA-3** | Consistencia fuerte en pagos: confirmación atómica de cobro + activación |
| **US28** | Ver plan actual y estado de suscripción |
| **US31** | Renovar plan activo con pago |
| **CON-1** | Enfoque de microservicios |

**Nuevos patrones esperados:**
- **Transactional Outbox Pattern** — Garantizar que eventos de "suscripción activada" no se pierdan
- **Idempotency Keys** — Evitar cobros duplicados en webhooks de Stripe
- **External Payment Gateway (Webhooks)** — Stripe como sistema externo de pagos

---

> **Documento generado para el curso de Fundamentos de Arquitectura de Software — UPC**
> 
> **Proyecto:** IoBuild — Iteración 2 (Cierre)
> **Fecha:** Mayo 2026
> **Estado:** ✅ 48/48 tests pasando, pipeline IoT operativo
