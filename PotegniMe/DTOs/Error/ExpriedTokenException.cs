namespace PotegniMe.DTOs.Error
{
    public class ExpiredTokenException(string message) : Exception(message)
    {
    }
}
