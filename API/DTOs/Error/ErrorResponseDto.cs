namespace API.DTOs.Error
{
    public class ErrorResponseDto
    {
        public required int ErrorCode { get; set; }
        public required string Message { get; set; }
    }
}