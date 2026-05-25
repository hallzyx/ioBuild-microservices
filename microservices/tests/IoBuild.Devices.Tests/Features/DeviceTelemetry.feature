Feature: Device Telemetry
  Como un Property Manager
  Quiero consultar la telemetria de mis dispositivos IoT
  Para monitorear el consumo de energia y el estado en tiempo real

  Background:
    Given el usuario esta autenticado como "PropertyManager"
    And existe un dispositivo con ID 1

  @US12
  Scenario: Consultar consumo de energia por hora de un dispositivo
    Given el dispositivo 1 tiene datos de telemetria en las ultimas 24 horas
    When el usuario envia una solicitud GET a "/api/v1/devices/1/energy"
    Then la respuesta debe tener codigo 200 OK
    And la respuesta debe contener una lista de puntos de energia
    And cada punto debe incluir los campos: timestamp, energyKwh, temperatureC, voltageV

  @US33
  Scenario: Consultar el ultimo estado conocido de un dispositivo
    Given el dispositivo 1 tiene telemetria con estado "online"
    When el usuario envia una solicitud GET a "/api/v1/devices/1/status"
    Then la respuesta debe tener codigo 200 OK
    And la respuesta debe contener el estado "online"
    And la respuesta debe incluir el campo lastSeen

  Scenario: Consultar energia de un dispositivo sin datos de telemetria
    Given el dispositivo 1 no tiene datos de telemetria
    When el usuario envia una solicitud GET a "/api/v1/devices/1/energy"
    Then la respuesta debe tener codigo 200 OK
    And la respuesta debe ser una lista vacia

  @Security
  Scenario: Usuario no autenticado no puede consultar telemetria
    Given el usuario no esta autenticado
    When el usuario envia una solicitud GET a "/api/v1/devices/1/energy"
    Then la respuesta debe tener codigo 401 Unauthorized
