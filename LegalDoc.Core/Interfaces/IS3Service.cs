namespace LegalDoc.Core.Interfaces
{
    /// <summary>
    /// Service for managing file operations with AWS S3
    /// </summary>
    public interface IS3Service
    {
        /// <summary>Upload a file and return the S3 key</summary>
        Task<string> UploadFileAsync(Stream fileStream, string key, string contentType);

        /// <summary>Generate a pre-signed URL for downloading (default 60 min)</summary>
        Task<string> GenerateDownloadUrlAsync(string key, TimeSpan? expiry = null);

        /// <summary>Generate a pre-signed URL for direct client upload</summary>
        Task<string> GenerateUploadUrlAsync(string key, string contentType, TimeSpan? expiry = null);

        /// <summary>Delete a file from S3</summary>
        Task<bool> DeleteFileAsync(string key);

        /// <summary>Check if a file exists</summary>
        Task<bool> FileExistsAsync(string key);

        /// <summary>Copy a file within S3 (template → contract)</summary>
        Task<string> CopyFileAsync(string sourceKey, string destinationKey);
    }
}
