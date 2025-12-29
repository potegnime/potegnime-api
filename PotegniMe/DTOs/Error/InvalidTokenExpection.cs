namespace PotegniMe.DTOs.Error
{
    public class InvalidTokenException(string message) : Exception(message)
    {
    }
}
