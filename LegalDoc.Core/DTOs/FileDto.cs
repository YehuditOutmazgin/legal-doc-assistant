namespace LegalDoc.Core.DTOs
{
    /// <summary>Returned when client needs to download a file</summary>
    public class FileDownloadDto
    {
        public string PresignedUrl { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }

    /// <summary>Returned when client needs to upload a file directly to S3</summary>
    public class FileUploadUrlDto
    {
        public string PresignedUrl { get; set; } = string.Empty;
        public string S3Key { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
