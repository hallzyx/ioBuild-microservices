Feature: Gestion de Dispositivos IoT
  Como un Property Manager
  Quiero listar y monitorear los dispositivos IoT registrados
  Para conocer su estado y ubicacion en tiempo real

  Background:
    Given el usuario esta autenticado como "PropertyManager"
    And existen 3 dispositivos registrados en el proyecto "Edificio Green Tower"

  @US33 @QA-2
  Scenario: Listar todos los dispositivos de un proyecto
    When el usuario envia una solicitud GET a "/api/v1/devices"
    Then la respuesta debe tener codigo 200 OK
    And la respuesta debe contener una lista de dispositivos
    And cada dispositivo debe tener los campos: id, name, type, location, status, projectId

  @US33 @QA-2
  Scenario: Obtener detalle de un dispositivo por ID
    Given existe un dispositivo con ID 1 y nombre "Termostato Lobby"
    When el usuario envia una solicitud GET a "/api/v1/devices/1"
    Then la respuesta debe tener codigo 200 OK
    And la respuesta debe contener el nombre "Termostato Lobby"

  @QA-2 @Security
  Scenario: Usuario no autenticado no puede listar dispositivos
    Given el usuario no esta autenticado
    When el usuario envia una solicitud GET a "/api/v1/devices"
    Then la respuesta debe tener codigo 401 Unauthorized
