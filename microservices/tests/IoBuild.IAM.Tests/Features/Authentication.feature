Feature: Autenticacion de Usuarios
  Como usuario del sistema IoBuild
  Quiero poder autenticarme con mi email y contrasena
  Para acceder a las funcionalidades segun mi rol

  Background:
    Given el sistema tiene un usuario registrado con email "builder@iobuilt.com"
    And su rol es "PropertyManager"
    And su contrasena es "SecurePass123!"

  @US44 @QA-1 @Critical
  Scenario: Inicio de sesion exitoso con credenciales validas
    Given el usuario no esta autenticado
    When el usuario envia una solicitud POST a "/api/v1/authentication/sign-in"
      | Field    | Value                  |
      | email    | builder@iobuilt.com    |
      | password | SecurePass123!         |
    Then la respuesta debe tener codigo 200 OK
    And la respuesta debe contener un token JWT valido
    And el token debe expirar en 7 dias
    And el token debe incluir el Claim "role" con valor "PropertyManager"

  @US44 @QA-1 @Security
  Scenario: Inicio de sesion fallido por contrasena incorrecta
    Given el usuario no esta autenticado
    When el usuario envia una solicitud POST a "/api/v1/authentication/sign-in"
      | Field    | Value                  |
      | email    | builder@iobuilt.com    |
      | password | WrongPassword456!      |
    Then la respuesta debe tener codigo 401 Unauthorized

  @QA-1 @Security
  Scenario: Acceso a endpoint protegido sin token retorna 401
    Given el usuario no esta autenticado
    When el usuario envia una solicitud GET a "/api/v1/users"
    Then la respuesta debe tener codigo 401 Unauthorized
