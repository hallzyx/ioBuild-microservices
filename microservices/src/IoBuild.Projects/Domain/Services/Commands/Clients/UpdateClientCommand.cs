using IoBuild.Projects.Domain.Model.ValueObjects;

namespace IoBuild.Projects.Domain.Services.Commands.Clients;

public class UpdateClientCommand
{
    public int Id { get; }
    public string FullName { get; }
    public string ProjectName { get; }
    public EAccountStatement AccountStatement { get; }
    public string Email { get; }
    public string PhoneNumber { get; }
    public string Address { get; }

    public UpdateClientCommand(int id, string fullName, string projectName, EAccountStatement accountStatement, string email, string phoneNumber, string address)
    {
        Id = id;
        FullName = fullName;
        ProjectName = projectName;
        AccountStatement = accountStatement;
        Email = email;
        PhoneNumber = phoneNumber;
        Address = address;
    }
}
