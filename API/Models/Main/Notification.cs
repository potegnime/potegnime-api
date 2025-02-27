using System.ComponentModel.DataAnnotations;

namespace API.Models.Main;

public class Notification
{
    [Key]
    public required int NotificationId { get; set; }

    [Required]
    public required string Title { get; set; }

    [Required]
    public required string Description { get; set; }

    [Required]
    public required NotificationType NotificationType { get; set; }

    public virtual ICollection<UserNotification> UserNotification { get; } = new List<UserNotification>();

}
