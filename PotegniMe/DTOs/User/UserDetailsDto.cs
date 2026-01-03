namespace PotegniMe.DTOs.User;

public class UserDetailsDto
{
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required DateOnly Joined { get; set; }
    public required string Role { get; set; }
    public required bool HasPfp { get; set; }
}