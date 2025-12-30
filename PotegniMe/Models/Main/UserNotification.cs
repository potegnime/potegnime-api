namespace PotegniMe.Models.Main;

public class UserNotification
{
    public int UserNotificationId { get; set; }

    public int UserId { get; set; }

    public int NotificationId { get; set; }

    public bool IsRead { get; set; } // Has default value

    public DateTime SentAt { get; set; } // Has default value

    public DateTime? ReadAt { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;

    public Notification Notification { get; set; } = null!;
}
