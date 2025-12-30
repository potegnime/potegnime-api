using System.ComponentModel.DataAnnotations.Schema;

namespace PotegniMe.Models.Main;

public class RoleRequest
{
    public int RoleRequestId { get; set; }

    public int RequestUserId { get; set; }

    public int RequestedRoleId { get; set; }

    public RoleRequestStatus Status { get; set; }

    public string? Message { get; set; }

    public  DateTime RequestDate { get; set; }

    public DateTime? CloseDate { get; set; }

    public int? AdminUserId { get; set; }

    // Navigation properties
    [ForeignKey("RequestUserId")]
    public User RequestUser { get; set; } = null!;

    [ForeignKey("RequestedRoleId")]
    public Role RequestedRole { get; set; } = null!;

    [ForeignKey("AdminUserId")]
    public User? AdminUser { get; set; } = null!;
}
