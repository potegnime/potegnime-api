namespace API.DTOs.FileSystem
{
    public class FileDto
    {
        public required string FilePath { get; set; }
        public required IFormFile File { get; set; }
    }
}
