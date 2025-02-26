namespace API.Services.FileService
{
    public interface IFileService
    {
        string ConvertFileToBase64(string filePath, FileSystemFileType fileType);

        string GetMimeType(string filePath);
    }
}
