namespace IoBuild.Projects.Domain.Services.Commands.Clients;

public class CreateClientCommand
{
    public string FullName { get; }
    public int ProjectId { get; }
    public string ProjectName { get; }
    public string Email { get; }
    public string PhoneNumber { get; }
    public string Address { get; }

    public CreateClientCommand(string fullName, int projectId, string projectName, string email, string phoneNumber, string address)
    {
        FullName = fullName;
        ProjectId = projectId;
        ProjectName = projectName;
        Email = email;
        PhoneNumber = phoneNumber;
        Address = address;
    }
}
