namespace PotegniMe.DTOs.Auth
{
    public class ResetPasswordDto
    {
        public required string Password { get; set; }
        public required string Token { get; set; }
    }
}
