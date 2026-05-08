using IoBuild.Projects.Domain.Model.ValueObjects;

namespace IoBuild.Projects.Domain.Model.Aggregates;

public class Client
{
    public int Id { get; private set; }
    public string FullName { get; private set; }
    public int ProjectId { get; private set; }
    public string ProjectName { get; private set; }
    public EAccountStatement AccountStatement { get; private set; }
    public string Email { get; private set; }
    public string PhoneNumber { get; private set; }
    public string Address { get; private set; }

    protected Client() { }

    public Client(string fullName, int projectId, string projectName, string email, string phoneNumber, string address)
    {
        FullName = fullName;
        ProjectId = projectId;
        ProjectName = projectName;
        AccountStatement = EAccountStatement.Pending;
        Email = email;
        PhoneNumber = phoneNumber;
        Address = address;
    }

    public void Update(string fullName, string projectName, EAccountStatement accountStatement, string email, string phoneNumber, string address)
    {
        FullName = fullName;
        ProjectName = projectName;
        AccountStatement = accountStatement;
        Email = email;
        PhoneNumber = phoneNumber;
        Address = address;
    }
}
