namespace LegalDoc.Core.Interfaces
{
    public interface IS3Service
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
        Task<Stream> DownloadFileAsync(string key);
        Task<bool> DeleteFileAsync(string key);
        Task<bool> FileExistsAsync(string key);
        Task<string> GeneratePresignedUrlAsync(string key, TimeSpan expiration);
    }
}