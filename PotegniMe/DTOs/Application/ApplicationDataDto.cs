using PotegniMe.DTOs.User;

namespace PotegniMe.DTOs.Application;

public class ApplicationDataDto
{
    public required UserDetailsDto User { get; set; }
    public required string Language { get; set; }
}