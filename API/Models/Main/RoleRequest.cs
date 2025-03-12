using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models.Main;

public class RoleRequest
{
    [Key]
    public required int RoleRequestId { get; set; }

    [Required]
    public required int RequestUserId { get; set; }

    [Required]
    public required int RequestedRoleId { get; set; }

    [Required]
    public required RoleRequestStatus Status { get; set; }

    public string? Message { get; set; }

    [Required]
    public required DateTime RequestDate { get; set; }

    public DateTime? CloseDate { get; set; }

    public int? AdminUserId { get; set; }

    // Navigation properties
    [ForeignKey("RequestUserId")]
    public virtual User RequestUser { get; set; } = null!;

    [ForeignKey("RequestedRoleId")]
    public virtual Role RequestedRole { get; set; } = null!;

    [ForeignKey("AdminUserId")]
    public virtual User? AdminUser { get; set; }
}
