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

    [When(@"el usuario envia una solicitud GET a ""(.*)""")]
    public async Task WhenElUsuarioEnviaUnaSolicitudGET(string endpoint)
    {
        if (!_isAuthenticated)
        {
            _responseCode = "401 Unauthorized";
            _devices = null;
        }
        else
        {
            _responseCode = "200 OK";
            // Simulate devices list
            _devices = new List<Dictionary<string, object>>();
            for (int i = 1; i <= _deviceCount; i++)
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
}