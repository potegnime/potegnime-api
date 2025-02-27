using Npgsql.PostgresTypes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace API.Models.Main;
public class User
{
    [Key]
    public int UserId { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Username { get; set; }

    [Required]
    [MaxLength(320)]
    public required string Email { get; set; }

    [Required]
    public string AuthToken { get; set; } // Has default value

    [Required]
    public required string PasswordHash { get; set; }

    [Required]
    public required string PasswordSalt { get; set; }

    public string? ProfilePicFilePath { get; set; }

    public string? ProfilePicUrl { get; set; }

    [Required]
    public required DateTime JoinedDate { get; set; }

    [Required]
    public int UploadedTorrentsCount { get; set; } // Has default value

    [Required]
    public int DownloadedTorrentsCount { get; set; } // Has default value

    [Required]
    public int LikedTorrentsCount { get; set; } // Has default value

    [Required]
    public int UploadBytes { get; set; } // Has default value

    [Required]
    public int DownloadBytes { get; set; } // Has default value

    public float? DonatedEur { get; set; }

    [Required]
    public required int RoleId { get; set; }

    // Navigation properties
    [ForeignKey("RoleId")]
    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<UserNotification> UserNotification { get; } = new List<UserNotification>();
}
