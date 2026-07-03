namespace IoBuild.IAM.Interfaces.REST.Resources;

public record UserResource(int Id, string Email, string Role);

public record GetAllUsersQuery();
public record GetUserByIdQuery(int UserId);
public record GetUserDetailByIdQuery(int UserId);
