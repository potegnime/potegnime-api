using System.ComponentModel.DataAnnotations;

namespace PotegniMe.Models.Main;

public class UserNotification
{
    [Required]
    public required int UserNotificationId { get; set; }

    [Required]
    public required int UserId { get; set; }

    [Required]
    public required int NotificationId { get; set; }

    [Required]
    public required bool IsRead { get; set; } // Has default value

    [Required]
    public required DateTime SentAt { get; set; } // Has default value

    public DateTime? ReadAt { get; set; }


    // Navigation properties
    public virtual User User { get; set; } = null!;

    public virtual Notification Notification { get; set; } = null!;
}
