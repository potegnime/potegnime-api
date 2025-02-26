namespace API.DTOs.Error
{
    public class ConflictExceptionDto : Exception
    {
        public ConflictExceptionDto(string message) : base(message)
        {
        }
    }
}
