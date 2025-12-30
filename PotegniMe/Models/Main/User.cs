using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PotegniMe.Models.Main;
public class User
{
    public int UserId { get; set; }

    [MaxLength(100)]
    public required string Username { get; set; }

    [MaxLength(320)]
    public required string Email { get; set; }

    [MaxLength(256)]
    public required string PasskeyCipher { get; set; }

    public required string PasswordHash { get; set; }

    public required string PasswordSalt { get; set; }

    public string? ProfilePicFilePath { get; set; }

    public DateTime JoinedDate { get; set; } = DateTime.UtcNow;

    public int UploadedTorrentsCount { get; set; } = 0;

    public int DownloadedTorrentsCount { get; set; } = 0;

    public int LikedTorrentsCount { get; set; } = 0;

    public long UploadBytes { get; set; } = 0;

    public long DownloadBytes { get; set; } = 0;

    public string? PasswordResetToken { get; set; }

    public DateTime? PasswordResetTokenExpiration { get; set; }

    public int RoleId { get; set; }

    // Navigation properties
    [ForeignKey("RoleId")]
    public Role Role { get; set; } = null!;

    public ICollection<UserNotification> UserNotifications { get; } = new List<UserNotification>();
}
