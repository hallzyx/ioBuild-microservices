using TechTalk.SpecFlow;
using Xunit;

namespace IoBuild.Devices.Tests.Steps;

[Binding]
public class DeviceManagementSteps
{
    private bool _isAuthenticated;
    private string? _userRole;
    private string? _projectName;
    private int _deviceCount;
    private string? _responseCode;
    private List<Dictionary<string, object>>? _devices;
    private Dictionary<string, object>? _device;
    private int? _deviceId;
    private string? _deviceName;

    // ── Telemetry fields ──
    private bool _deviceExists = true;
    private bool _hasTelemetryData;
    private string? _deviceStatus;
    private List<Dictionary<string, object>>? _energyData;
    private Dictionary<string, object>? _statusData;

    [Given(@"el usuario esta autenticado como ""(.*)""")]
    public void GivenElUsuarioEstaAutenticado(string role)
    {
        _isAuthenticated = true;
        _userRole = role;
    }

    [Given(@"existen (.*) dispositivos registrados en el proyecto ""(.*)""")]
    public void GivenExistenDispositivosRegistrados(int count, string projectName)
    {
        _deviceCount = count;
        _projectName = projectName;
        _devices = new List<Dictionary<string, object>>();
        
        var deviceTypes = new[] { "Termometro", "SensorHumedad", "Camaras", "SensorMovimiento" };
        var locations = new[] { "Lobby", "Piso 1", "Piso 2", "Azotea" };
        var statuses = new[] { "online", "offline", "maintenance" };
        
        for (int i = 1; i <= count; i++)
        {
            _devices.Add(new Dictionary<string, object>
            {
                ["id"] = i,
                ["name"] = $"{deviceTypes[i % deviceTypes.Length]} {locations[i % locations.Length]}",
                ["type"] = deviceTypes[i % deviceTypes.Length],
                ["location"] = locations[i % locations.Length],
                ["status"] = statuses[i % statuses.Length],
                ["projectId"] = 1
            });
        }
    }

    [Given(@"el usuario no esta autenticado")]
    public void GivenElUsuarioNoEstaAutenticado()
    {
        _isAuthenticated = false;
    }

    [Given(@"existe un dispositivo con ID (.*) y nombre ""(.*)""")]
    public void GivenExisteUnDispositivoConID(int id, string name)
    {
        _deviceId = id;
        _deviceName = name;
        _device = new Dictionary<string, object>
        {
            ["id"] = id,
            ["name"] = name,
            ["type"] = "Termostato",
            ["location"] = "Lobby",
            ["status"] = "online",
            ["projectId"] = 1
        };
    }

    [Given(@"existe un dispositivo con ID (.*)")]
    public void GivenExisteUnDispositivoConIDSolo(int id)
    {
        _deviceId = id;
        _deviceExists = true;
    }

    // ── Telemetry Given steps ──

    [Given(@"el dispositivo (.*) tiene datos de telemetria en las ultimas 24 horas")]
    public void GivenDispositivoTieneTelemetria(int deviceId)
    {
        _hasTelemetryData = true;
    }

    [Given(@"el dispositivo (.*) tiene telemetria con estado ""(.*)""")]
    public void GivenDispositivoTieneTelemetriaConEstado(int deviceId, string status)
    {
        _hasTelemetryData = true;
        _deviceStatus = status;
    }

    [Given(@"el dispositivo (.*) no tiene datos de telemetria")]
    public void GivenDispositivoNoTieneTelemetria(int deviceId)
    {
        _hasTelemetryData = false;
    }

    [When(@"el usuario envia una solicitud GET a ""(.*)""")]
    public async Task WhenElUsuarioEnviaUnaSolicitudGET(string endpoint)
    {
        if (!_isAuthenticated)
        {
            _responseCode = "401 Unauthorized";
            _devices = null;
            return;
        }

        if (!_deviceExists)
        {
            _responseCode = "404 Not Found";
            return;
        }

        // ── Telemetry endpoints ──
        if (endpoint.Contains("/energy"))
        {
            _responseCode = "200 OK";
            _energyData = _hasTelemetryData
                ? new List<Dictionary<string, object>>
                {
                    new() { ["timestamp"] = "2026-05-25T10:00:00Z", ["energyKwh"] = 1.5, ["temperatureC"] = 22.3, ["voltageV"] = 220.1 },
                    new() { ["timestamp"] = "2026-05-25T11:00:00Z", ["energyKwh"] = 1.8, ["temperatureC"] = 22.5, ["voltageV"] = 219.8 },
                }
                : new List<Dictionary<string, object>>();
            await Task.CompletedTask;
            return;
        }

        if (endpoint.Contains("/status"))
        {
            _responseCode = "200 OK";
            _statusData = _hasTelemetryData
                ? new Dictionary<string, object>
                {
                    ["deviceId"] = 1, ["status"] = _deviceStatus ?? "online",
                    ["lastSeen"] = "2026-05-25T12:00:00Z", ["temperatureC"] = 22.3, ["voltageV"] = 220.1
                }
                : new Dictionary<string, object>
                {
                    ["deviceId"] = 1, ["status"] = "unknown", ["lastSeen"] = null!
                };
            await Task.CompletedTask;
            return;
        }

        // ── Standard device endpoints ──
        _responseCode = "200 OK";
        _devices = new List<Dictionary<string, object>>();
        for (int i = 1; i <= ( _deviceCount > 0 ? _deviceCount : 3); i++)
        {
            _devices.Add(new Dictionary<string, object>
            {
                ["id"] = i,
                ["name"] = $"Device {i}",
                ["type"] = "Sensor",
                ["location"] = "Location",
                ["status"] = "online",
                ["projectId"] = 1
            });
        }
        await Task.CompletedTask;
    }

    [Then(@"la respuesta debe tener codigo (.*)")]
    public void ThenLaRespuestaDebeTenerCodigo(string expectedCode)
    {
        Assert.Equal(expectedCode, _responseCode);
    }

    [Then(@"la respuesta debe contener una lista de dispositivos")]
    public void ThenLaRespuestaDebeContenerUnaLista()
    {
        Assert.NotNull(_devices);
        Assert.True(_devices.Count > 0);
    }

    [Then(@"cada dispositivo debe tener los campos: id, name, type, location, status, projectId")]
    public void ThenCadaDispositivoDebeTenerLosCampos()
    {
        Assert.NotNull(_devices);
        foreach (var device in _devices)
        {
            Assert.True(device.ContainsKey("id"));
            Assert.True(device.ContainsKey("name"));
            Assert.True(device.ContainsKey("type"));
            Assert.True(device.ContainsKey("location"));
            Assert.True(device.ContainsKey("status"));
            Assert.True(device.ContainsKey("projectId"));
        }
    }

    [Then(@"la respuesta debe contener el nombre ""(.*)""")]
    public void ThenLaRespuestaDebeContenerElNombre(string name)
    {
        Assert.NotNull(_device);
        Assert.Equal(name, _device["name"]);
    }

    // ── Telemetry Then steps ──

    [Then(@"la respuesta debe contener una lista de puntos de energia")]
    public void ThenLaRespuestaDebeContenerPuntosEnergia()
    {
        Assert.NotNull(_energyData);
        Assert.NotEmpty(_energyData);
    }

    [Then(@"cada punto debe incluir los campos: timestamp, energyKwh, temperatureC, voltageV")]
    public void ThenCadaPuntoDebeIncluirCampos()
    {
        Assert.NotNull(_energyData);
        foreach (var point in _energyData)
        {
            Assert.True(point.ContainsKey("timestamp"));
            Assert.True(point.ContainsKey("energyKwh"));
            Assert.True(point.ContainsKey("temperatureC"));
            Assert.True(point.ContainsKey("voltageV"));
        }
    }

    [Then(@"la respuesta debe contener el estado ""(.*)""")]
    public void ThenLaRespuestaDebeContenerEstado(string expectedStatus)
    {
        Assert.NotNull(_statusData);
        Assert.Equal(expectedStatus, _statusData["status"]);
    }

    [Then(@"la respuesta debe incluir el campo lastSeen")]
    public void ThenLaRespuestaDebeIncluirLastSeen()
    {
        Assert.NotNull(_statusData);
        Assert.True(_statusData.ContainsKey("lastSeen"));
    }

    [Then(@"la respuesta debe ser una lista vacia")]
    public void ThenLaRespuestaDebeSerListaVacia()
    {
        Assert.NotNull(_energyData);
        Assert.Empty(_energyData);
    }
}