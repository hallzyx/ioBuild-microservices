# Telemetry Display — Frontend Specification

## Purpose

Add a device detail view to IoBuild Frontend showing real-time device status and historical energy consumption telemetry. The view is reachable from the device management table and calls two backend endpoints authenticated via JWT.

## Requirements

### Requirement: Device Detail Route

The system MUST expose a route `/devices/:id/detail` that navigates to the device detail view. The route MUST require authentication.

#### Scenario: Navigate from table to detail

- GIVEN a user is viewing the device management table
- WHEN they click "View Details" on a device row
- THEN the browser navigates to `/devices/:id/detail`
- AND the detail view loads telemetry for that device

#### Scenario: Direct URL access

- GIVEN a user is authenticated
- WHEN they navigate directly to `/devices/42/detail`
- THEN the detail view loads device and telemetry data for device 42

#### Scenario: Unauthenticated access

- GIVEN a user is NOT authenticated
- WHEN they navigate to `/devices/:id/detail`
- THEN the system redirects to login

### Requirement: Energy Consumption Chart

The system MUST display a line chart of energy consumption (kWh) over time when the energy endpoint returns data.

#### Scenario: Chart renders with data

- GIVEN the energy endpoint returns an array of telemetry points for the device
- WHEN the detail view loads
- THEN a line chart renders with time on the X axis and energyKwh on the Y axis

#### Scenario: Chart displays empty state

- GIVEN the energy endpoint returns an empty array for the device
- WHEN the detail view loads
- THEN the chart area displays a message "No energy data available for this period"

#### Scenario: Chart loading indicator

- GIVEN the energy endpoint has not yet responded
- WHEN the detail view is mounting
- THEN a loading spinner is shown in the chart area

### Requirement: Device Status Card

The system MUST display a status card showing the device's real-time status, last-seen timestamp, temperature, and voltage obtained from the status endpoint.

#### Scenario: Status card shows live data

- GIVEN the status endpoint returns `{ status: "Online", lastSeen: "2025-05-25T10:00:00Z", temperatureC: 23.4, voltageV: 220.1 }`
- WHEN the detail view loads
- THEN the card shows "Online" with a green badge, lastSeen formatted as relative time, temperature "23.4 °C", and voltage "220.1 V"

#### Scenario: Status is unknown

- GIVEN the status endpoint returns `{ status: "unknown", lastSeen: null }`
- WHEN the detail view loads
- THEN the card shows "Unknown" with a gray badge and "N/A" for lastSeen, temperature, and voltage

#### Scenario: Status endpoint returns 404

- GIVEN the device does not exist
- WHEN the API returns a 404
- THEN the view displays a "Device not found" error message
- AND the user can navigate back to the device list

### Requirement: API Error Handling

The system MUST display user-facing error messages when telemetry API calls fail, without breaking the entire view.

#### Scenario: Energy endpoint fails

- GIVEN the energy endpoint throws a network error
- WHEN the detail view loads
- THEN the chart area displays an error message "Could not load energy data"
- AND the status card still attempts to load independently

#### Scenario: Status endpoint fails

- GIVEN the status endpoint throws a network error
- WHEN the detail view loads
- THEN the status card displays an error message "Could not load device status"
- AND the chart still attempts to load independently

### Requirement: JWT Authentication

All telemetry API calls MUST include the JWT Bearer token in the Authorization header via the existing Axios interceptor.

#### Scenario: Token attached to requests

- GIVEN the user has a valid JWT token in localStorage
- WHEN the detail view calls the energy or status endpoint
- THEN the request includes `Authorization: Bearer <token>`

### Requirement: Backward Compatibility

The existing device management list and table MUST continue to work unchanged. The new fields (lastSeen, temperatureC, voltageV) in the entity model MUST be optional and default to null.

#### Scenario: Existing list unaffected

- GIVEN the device entity model now has optional lastSeen, temperatureC, voltageV fields
- WHEN the device list endpoint returns data without these fields
- THEN the existing table renders without errors
- AND the fields default to null

### Requirement: Internationalization

The detail view text (labels, empty messages, errors) MUST use the existing vue-i18n system. The system SHALL support both English and Spanish.

#### Scenario: Detail view in Spanish

- GIVEN the user's locale is set to Spanish
- WHEN they view the device detail page
- THEN all labels, empty states, and error messages appear in Spanish

### Requirement: Navigation from Table

The device management table SHOULD include a "View Details" action per row. Clicking it SHOULD navigate to the detail view.

#### Scenario: View Details button in table

- GIVEN a user is on the device management page
- WHEN they view a device row
- THEN they see a "View Details" icon or button
- AND clicking it navigates to `/devices/:id/detail`

## Data Contracts

### Device Entity — Modified

```
Device {
  id, name, type, location, projectId, status, macAddress,  // existing
  lastSeen: string|null,       // ADDED — ISO 8601 from status endpoint
  temperatureC: number|null,   // ADDED — from status endpoint
  voltageV: number|null        // ADDED — from status endpoint
}
```

### toTelemetryList() — New Assembler Method

```
Input:  Array<{ timestamp: string, energyKwh: number, temperatureC: number, voltageV: number }>
Output: Array<{ timestamp: Date, energyKwh: number, temperatureC: number, voltageV: number }>
Rules:
  - timestamp: parse ISO string → Date; skip entry if invalid
  - numbers: parse as float, default to 0 if NaN
  - empty input → empty array
```

### Energy Endpoint

```
GET /api/v1/devices/{id}/energy?from=&to=
→ 200: Array<{ timestamp: string, energyKwh: number, temperatureC: number, voltageV: number }>
→ 404: { error: string }
→ Empty: []
```

### Status Endpoint

```
GET /api/v1/devices/{id}/status
→ 200: { deviceId: string, status: string, lastSeen: string|null, temperatureC: number|null, voltageV: number|null }
→ 404: { error: string }
→ Unknown: { status: "unknown", lastSeen: null }
```

## UI / UX Specifications

### Detail View Layout (`device-detail.view.vue`)

```
┌─────────────────────────────────────────────┐
│  ← Back to Devices            Device Name    │
├─────────────────────┬───────────────────────┤
│                     │                       │
│  STATUS CARD        │  ENERGY CHART         │
│  ┌───────────────┐  │  ┌─────────────────┐  │
│  │ Device: X      │  │  │  Line chart     │  │
│  │ Type: Y        │  │  │  X=timestamp    │  │
│  │ Location: Z    │  │  │  Y=energyKwh    │  │
│  │                │  │  │                 │  │
│  │ Status: ● Online│  │  │  (or empty     │  │
│  │ Last seen: 2m  │  │  │   state msg)    │  │
│  │ Temp: 23.4°C   │  │  │                 │  │
│  │ Voltage: 220V  │  │  └─────────────────┘  │
│  └───────────────┘  │                       │
│                     │                       │
└─────────────────────┴───────────────────────┘
```

### Status Card (`device-status-card.component.vue`)
- **Border**: Left accent border (green=Online, red=Offline, gray=Unknown)
- **Fields**: Device name, Type (translated), Location, Status badge (colored pv-tag), Last seen (relative time via timeago or computed), Temperature, Voltage
- **Error state**: Red banner "Could not load status"
- **Loading**: Skeletons or spinner

### Energy Chart (`energy-consumption-chart.component.vue`)
- **Type**: Line chart (Chart.js via vue-chartjs)
- **Dataset**: energyKwh as the primary line, blue color
- **Optional overlay**: temperatureC as a secondary dashed line (gray), toggleable
- **X axis**: time labels (auto-skip if too many points)
- **Y axis**: energyKwh with unit suffix
- **Empty state**: Centered icon + "No energy data available"
- **Loading**: Spinner overlay

## Test Strategy

| Layer | Tool | What to test |
|-------|------|-------------|
| Unit (assembler) | Vitest | `toTelemetryList()`: valid data, nulls, empty array, invalid timestamps |
| Unit (store) | Vitest | telemetry state transitions, error handling, loading flag |
| Unit (components) | Vitest + vue-test-utils | DeviceStatusCard renders correct badge, handles null fields; EnergyChart renders/destroys chart, shows empty state |
| Integration (view) | Vitest + vue-test-utils | device-detail view orchestrates API calls, passes props to children, renders error state on 404 |
| E2E | Playwright | Navigate from table → detail, verify chart renders, verify status card |
| Accessibility | axe-core | Check contrast, aria labels, keyboard navigation in detail view |

## Acceptance Criteria

| ID | Criterion | Pass/Fail |
|----|-----------|-----------|
| AC1 | Route `/devices/:id/detail` resolves and renders the detail view | |
| AC2 | Chart renders with data when energy endpoint returns points | |
| AC3 | Chart shows empty state message when energy endpoint returns `[]` | |
| AC4 | Status card shows Online/Offline/Unknown with correct badge color | |
| AC5 | Status card shows "N/A" when lastSeen, temperatureC, or voltageV are null | |
| AC6 | Error in one endpoint does not block the other endpoint from loading | |
| AC7 | "View Details" button exists in devices-table and navigates correctly | |
| AC8 | All telemetry API calls include JWT Bearer token | |
| AC9 | Existing device list and table render unchanged | |
| AC10 | Spanish and English labels display correctly based on locale | |

---

*Change: frontend-telemetry-display | Project: IoBuild Frontend (microservices/frontend-docker/IoBuild-Frontend)*
