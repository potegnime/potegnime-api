namespace PotegniMe.DTOs.Error
{
    public class ExpiredTokenException : Exception
    {
        public ExpiredTokenException(string message) : base(message)
        {
        }
    }
}
