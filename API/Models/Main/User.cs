using System.ComponentModel.DataAnnotations.Schema;
namespace API.Models.Main;
public class User
{
    public int UserId { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string PasswordSalt { get; set; }
    public string? ProfilePicFilePath { get; set; }
    public required DateTime JoinedDate { get; set; }
    public required int RoleId { get; set; }

    // 1-many
    [ForeignKey("RoleId")]
    public required virtual Role Role { get; set; } 

}
