namespace API.Models.Main;

public class Role
{
    public required int RoleId { get; set; }
    public required string Name { get; set; }

    // 1-many
    public virtual List<User> Users { get; set; } = new();
}
