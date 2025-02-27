using System.ComponentModel.DataAnnotations;

namespace API.Models.Main;

public class Role
{
    [Key]
    public required int RoleId { get; set; }

    [Required]
    public required string Name { get; set; }

    // Navigation properties
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
