using System.ComponentModel.DataAnnotations;

namespace PotegniMe.Models.Main;

public class Role
{
    public int RoleId { get; set; }

    [MaxLength(50)]
    public required string Name { get; set; }

    // Navigation properties
    public ICollection<User> Users { get; set; } = new List<User>();
}
