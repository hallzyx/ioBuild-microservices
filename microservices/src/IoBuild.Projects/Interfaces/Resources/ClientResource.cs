using IoBuild.Projects.Domain.Model.ValueObjects;

namespace IoBuild.Projects.Interfaces.Resources;

public class ClientResource
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public int ProjectId { get; set; }
    public string ProjectName { get; set; }
    public EAccountStatement AccountStatement { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Address { get; set; }
}
