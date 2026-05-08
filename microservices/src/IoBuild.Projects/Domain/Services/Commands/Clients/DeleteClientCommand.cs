namespace IoBuild.Projects.Domain.Services.Commands.Clients;

public class DeleteClientCommand
{
    public int Id { get; }

    public DeleteClientCommand(int id)
    {
        Id = id;
    }
}
