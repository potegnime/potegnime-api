namespace API.DTOs.User
{
    public class GetUser
    {
        public required int UserId { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Joined { get; set; }
        public required string Role { get; set; }
    }
}
