namespace PotegniMe.Models.Main;

public class Notification
{
    public int NotificationId { get; set; }

    public required string Title { get; set; }

    public required string Description { get; set; }

    public NotificationType NotificationType { get; set; }

    public ICollection<UserNotification> UserNotifications { get; } = new List<UserNotification>();
}
