namespace PotegniMe.DTOs.Error
{
    public class ConflictExceptionDto(string message) : Exception(message)
    {
    }
}
