Feature: Gestion de Proyectos y Clientes
  Como un Property Manager
  Quiero gestionar proyectos de construccion y sus clientes asociados
  Para administrar el portafolio de propiedades

  Background:
    Given el usuario esta autenticado como "PropertyManager"
    And existen 3 proyectos registrados en el sistema

  @CRN-1
  Scenario: Listar todos los proyectos
    When el usuario envia una solicitud GET a "/api/v1/projects"
    Then la respuesta debe tener codigo 200 OK
    And la respuesta debe contener una lista de proyectos
    And cada proyecto debe tener los campos: id, name, description, location, totalUnits, status

  @CRN-1
  Scenario: Crear un nuevo proyecto
    When el usuario envia una solicitud POST a "/api/v1/projects"
      | Field       | Value                |
      | name        | Edificio Green Tower |
      | description | Proyecto sostenible  |
      | location    | Miraflores           |
      | totalUnits  | 20                   |
    Then la respuesta debe tener codigo 201 Created
    And la respuesta debe incluir el ID del nuevo proyecto

  @CRN-1
  Scenario: Asignar un cliente a un proyecto
    Given existe un proyecto con ID 1
    When el usuario envia una solicitud POST a "/api/v1/clients"
      | Field        | Value               |
      | fullName     | Juan Perez          |
      | projectId    | 1                   |
      | email        | juan@example.com    |
    Then la respuesta debe tener codigo 201 Created
    And el cliente debe estar asociado al proyecto ID 1
