namespace IoBuild.Projects.Domain.Services.Commands.Projects;

public class DeleteProjectCommand
{
    public int Id { get; }

    public DeleteProjectCommand(int id)
    {
        Id = id;
    }
}
