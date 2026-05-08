namespace IoBuild.IAM.Domain.Model.Aggregates;

public class User
{
    public int Id { get; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string Role { get; private set; }

    public User(string email, string passwordHash, string role)
    {
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
    }

    public void UpdatePasswordHash(string newHash) => PasswordHash = newHash;
    public void UpdateEmail(string email) => Email = email;
    public void UpdateRole(string role) => Role = role;
}
