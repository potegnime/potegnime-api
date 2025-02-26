namespace API.DTOs.User
{
    public class UpdateRoleDto
    {
        public required int UserId { get; set; }
        public required string RoleName { get; set; }
    }
}