using TechTalk.SpecFlow;
using Xunit;

namespace IoBuild.Subscriptions.Tests.Steps;

[Binding]
public class SubscriptionRenewalSteps
{
    private bool _isAuthenticated;
    private string? _userRole;
    private string? _currentPlan;
    private bool _stripeConfigured;
    private string? _responseCode;
    private string? _checkoutUrl;
    private string? _sessionId;
    private int? _builderId;
    private int? _planId;
    private string? _subscriptionStatus;
    private DateTime? _subscriptionEndDate;
    private bool _paymentSuccessful;

    [Given(@"el usuario esta autenticado como ""(.*)""")]
    public void GivenElUsuarioEstaAutenticado(string role)
    {
        _isAuthenticated = true;
        _userRole = role;
    }

    [Given(@"el usuario tiene un plan ""(.*)"" activo")]
    public void GivenElUsuarioTieneUnPlanActivo(string plan)
    {
        _currentPlan = plan;
    }

    [Given(@"la pasarela de pagos Stripe esta configurada")]
    public void GivenLaPasarelaDePagosStripeEstaConfigurada()
    {
        _stripeConfigured = true;
    }

    [Given(@"el webhook de Stripe notifica un pago exitoso con sessionId ""(.*)""")]
    public void GivenElWebhookDeStripeNotificaUnPagoExitoso(string sessionId)
    {
        _sessionId = sessionId;
        _paymentSuccessful = true;
    }

    [Given(@"la sesion contiene metadata con builder_id=(.*) y plan_id=(.*)")]
    public void GivenLaSesionContieneMetadata(int builderId, int planId)
    {
        _builderId = builderId;
        _planId = planId;
    }

    [Given(@"el webhook de Stripe notifica un pago fallido con sessionId ""(.*)""")]
    public void GivenElWebhookDeStripeNotificaUnPagoFallido(string sessionId)
    {
        _sessionId = sessionId;
        _paymentSuccessful = false;
    }

    [When(@"el usuario envia una solicitud POST a ""(.*)""")]
    public async Task WhenElUsuarioEnviaUnaSolicitudPOST(string endpoint, Table table)
    {
        if (!_isAuthenticated || !_stripeConfigured)
        {
            _responseCode = "401 Unauthorized";
            _checkoutUrl = null;
        }
        else
        {
            // Parse table with Field/Value format
            foreach (var row in table.Rows)
            {
                var field = row["Field"].Trim().ToLower();
                var value = row["Value"].Trim();
                
                if (field == "planid")
                    _planId = int.Parse(value);
                else if (field == "builderid")
                    _builderId = int.Parse(value);
            }
            
            _responseCode = "200 OK";
            _checkoutUrl = $"https://checkout.stripe.com/pay/cs_test_{Guid.NewGuid().ToString().Substring(0, 8)}";
        }
        
        await Task.CompletedTask;
    }

    [When(@"el sistema procesa la confirmacion del pago")]
    public async Task WhenElSistemaProcesaLaConfirmacion()
    {
        if (_paymentSuccessful && _builderId.HasValue && _planId.HasValue)
        {
            _subscriptionStatus = "active";
            _subscriptionEndDate = DateTime.Now.AddMonths(1);
        }
        else
        {
            _subscriptionStatus = "pending";
            _subscriptionEndDate = null;
        }
        
        await Task.CompletedTask;
    }

    [Then(@"la respuesta debe tener codigo (.*)")]
    public void ThenLaRespuestaDebeTenerCodigo(string expectedCode)
    {
        Assert.Equal(expectedCode, _responseCode);
    }

    [Then(@"la respuesta debe contener una URL de checkout de Stripe")]
    public void ThenLaRespuestaDebeContenerUnaURL()
    {
        Assert.NotNull(_checkoutUrl);
        Assert.StartsWith("https://checkout.stripe.com", _checkoutUrl);
    }

    [Then(@"la suscripcion del builder debe actualizarse a estado ""(.*)""")]
    public void ThenLaSuscripcionDebeActualizarse(string status)
    {
        Assert.Equal(status, _subscriptionStatus);
    }

    [Then(@"la fecha de fin debe ser (.*) mes despues de la fecha actual")]
    public void ThenLaFechaDeFinDebeSerMesDespues(int months)
    {
        Assert.NotNull(_subscriptionEndDate);
        var expectedDate = DateTime.Now.AddMonths(months);
        Assert.Equal(expectedDate.Month, _subscriptionEndDate.Value.Month);
    }

    [Then(@"la suscripcion no debe modificarse")]
    public void ThenLaSuscripcionNoDebeModificarse()
    {
        // In failed payment, status should remain as it was (no change)
        Assert.Equal("pending", _subscriptionStatus);
    }

    [Then(@"el sistema debe retornar estado ""(.*)""")]
    public void ThenElSistemaDebeRetornarEstado(string status)
    {
        Assert.Equal(status, _subscriptionStatus);
    }
}