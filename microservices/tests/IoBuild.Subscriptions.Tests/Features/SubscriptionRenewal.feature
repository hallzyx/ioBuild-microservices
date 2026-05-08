Feature: Renovacion de Plan de Suscripcion
  Como un Property Manager
  Quiero renovar mi plan activo
  Para asegurar la continuidad del servicio

  Background:
    Given el usuario esta autenticado como "PropertyManager"
    And el usuario tiene un plan "Professional" activo
    And la pasarela de pagos Stripe esta configurada

  @US31 @QA-3 @Payment
  Scenario: Crear sesion de pago para renovar plan
    When el usuario envia una solicitud POST a "/api/v1/subscriptions/payments/create-session"
      | Field    | Value                |
      | planId   | 2                    |
      | builderId| 1                    |
    Then la respuesta debe tener codigo 200 OK
    And la respuesta debe contener una URL de checkout de Stripe

  @QA-3 @Payment @Critical
  Scenario: Confirmar pago exitoso y activar suscripcion
    Given el webhook de Stripe notifica un pago exitoso con sessionId "cs_test_abc123"
    And la sesion contiene metadata con builder_id=1 y plan_id=2
    When el sistema procesa la confirmacion del pago
    Then la suscripcion del builder debe actualizarse a estado "active"
    And la fecha de fin debe ser 1 mes despues de la fecha actual

  @QA-3 @Payment
  Scenario: Webhook con pago fallido no activa la suscripcion
    Given el webhook de Stripe notifica un pago fallido con sessionId "cs_test_failed"
    When el sistema procesa la confirmacion del pago
    Then la suscripcion no debe modificarse
    And el sistema debe retornar estado "pending"
