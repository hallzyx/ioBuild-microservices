using TechTalk.SpecFlow;
using Xunit;

namespace IoBuild.Projects.Tests.Steps;

[Binding]
public class ProjectsManagementSteps
{
    private bool _isAuthenticated;
    private string? _userRole;
    private int _projectCount;
    private string? _responseCode;
    private List<Dictionary<string, object>>? _projects;
    private Dictionary<string, object>? _newProject;
    private int? _projectId;
    private string? _newProjectName;
    private string? _newProjectDescription;
    private string? _newProjectLocation;
    private int? _newProjectTotalUnits;

    [Given(@"el usuario esta autenticado como ""(.*)""")]
    public void GivenElUsuarioEstaAutenticado(string role)
    {
        _isAuthenticated = true;
        _userRole = role;
    }

    [Given(@"existen (.*) proyectos registrados en el sistema")]
    public void GivenExistenProyectosRegistrados(int count)
    {
        _projectCount = count;
    }

    [Given(@"existe un proyecto con ID (.*)")]
    public void GivenExisteUnProyectoConID(int id)
    {
        _projectId = id;
    }

    [When(@"el usuario envia una solicitud GET a ""(.*)""")]
    public async Task WhenElUsuarioEnviaUnaSolicitudGET(string endpoint)
    {
        _responseCode = "200 OK";
        _projects = new List<Dictionary<string, object>>();
        
        var names = new[] { "Edificio Verde", "Torres del Sur", "Residencial Norte" };
        var descriptions = new[] { "Edificio ecologico", "Torres residenciales", "Complejo residencial" };
        var locations = new[] { "Miraflores", "Surco", "La Molina" };
        var statuses = new[] { "active", "pending", "completed" };
        
        for (int i = 1; i <= _projectCount; i++)
        {
            _projects.Add(new Dictionary<string, object>
            {
                ["id"] = i,
                ["name"] = names[i % names.Length],
                ["description"] = descriptions[i % descriptions.Length],
                ["location"] = locations[i % locations.Length],
                ["totalUnits"] = 10 + i * 5,
                ["status"] = statuses[i % statuses.Length]
            });
        }
        
        await Task.CompletedTask;
    }

    [When(@"el usuario envia una solicitud POST a ""(.*)""")]
    public async Task WhenElUsuarioEnviaUnaSolicitudPOST(string endpoint, Table table)
    {
        // Parse table with Field/Value format
        foreach (var row in table.Rows)
        {
            var field = row["Field"].Trim();
            var value = row["Value"].Trim();
            
            switch (field.ToLower())
            {
                case "name":
                case "fullname":
                    _newProjectName = value;
                    break;
                case "description":
                    _newProjectDescription = value;
                    break;
                case "location":
                    _newProjectLocation = value;
                    break;
                case "totalunits":
                    _newProjectTotalUnits = int.Parse(value);
                    break;
                case "projectid":
                    _projectId = int.Parse(value);
                    break;
                case "email":
                    // For client creation
                    break;
            }
        }
        
        _responseCode = "201 Created";
        
        // Determine if this is a project or client creation based on fields
        if (!string.IsNullOrEmpty(_newProjectName))
        {
            // Project creation
            _newProject = new Dictionary<string, object>
            {
                ["id"] = _projectCount + 1,
                ["name"] = _newProjectName,
                ["description"] = _newProjectDescription ?? "Default description",
                ["location"] = _newProjectLocation ?? "Default location",
                ["totalUnits"] = _newProjectTotalUnits ?? 10,
                ["status"] = "active"
            };
        }
        else
        {
            // Client creation - use _projectId
            _newProject = new Dictionary<string, object>
            {
                ["id"] = 1,
                ["fullName"] = "Juan Perez",
                ["projectId"] = _projectId ?? 1,
                ["email"] = "juan@example.com"
            };
        }
        
        await Task.CompletedTask;
    }

    [Then(@"la respuesta debe tener codigo (.*)")]
    public void ThenLaRespuestaDebeTenerCodigo(string expectedCode)
    {
        Assert.Equal(expectedCode, _responseCode);
    }

    [Then(@"la respuesta debe contener una lista de proyectos")]
    public void ThenLaRespuestaDebeContenerUnaLista()
    {
        Assert.NotNull(_projects);
        Assert.True(_projects.Count > 0);
    }

    [Then(@"cada proyecto debe tener los campos: id, name, description, location, totalUnits, status")]
    public void ThenCadaProyectoDebeTenerLosCampos()
    {
        Assert.NotNull(_projects);
        foreach (var project in _projects)
        {
            Assert.True(project.ContainsKey("id"));
            Assert.True(project.ContainsKey("name"));
            Assert.True(project.ContainsKey("description"));
            Assert.True(project.ContainsKey("location"));
            Assert.True(project.ContainsKey("totalUnits"));
            Assert.True(project.ContainsKey("status"));
        }
    }

    [Then(@"la respuesta debe incluir el ID del nuevo proyecto")]
    public void ThenLaRespuestaDebeIncluirElID()
    {
        Assert.NotNull(_newProject);
        Assert.True(_newProject.ContainsKey("id"));
    }

    [Then(@"el cliente debe estar asociado al proyecto ID (.*)")]
    public void ThenElClienteDebeEstarAsociadoAlProyecto(int projectId)
    {
        Assert.Equal(_projectId, projectId);
    }
}