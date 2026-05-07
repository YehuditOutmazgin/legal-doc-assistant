using Amazon.S3;
using Amazon.S3.Model;
using LegalDoc.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LegalDoc.Infrastructure.Services
{
    public class S3Service : IS3Service
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        private readonly int _presignedUrlExpiryMinutes;
        private readonly ILogger<S3Service> _logger;

        public S3Service(IAmazonS3 s3Client, IConfiguration config, ILogger<S3Service> logger)
        {
            _s3Client = s3Client;
            _bucketName = config["AWS:BucketName"]!;
            _presignedUrlExpiryMinutes = int.Parse(config["S3Settings:PresignedUrlExpiryMinutes"]!);
            _logger = logger;
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string key, string contentType)
        {
            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = fileStream,
                ContentType = contentType,
                ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
            };

            await _s3Client.PutObjectAsync(request);
            _logger.LogInformation("Uploaded file to S3: {Key}", key);
            return key;
        }

        public async Task<string> GenerateDownloadUrlAsync(string key, TimeSpan? expiry = null)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = key,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.Add(expiry ?? TimeSpan.FromMinutes(_presignedUrlExpiryMinutes))
            };

            return await Task.FromResult(_s3Client.GetPreSignedURL(request));
        }

        public async Task<string> GenerateUploadUrlAsync(string key, string contentType, TimeSpan? expiry = null)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = key,
                Verb = HttpVerb.PUT,
                ContentType = contentType,
                Expires = DateTime.UtcNow.Add(expiry ?? TimeSpan.FromMinutes(_presignedUrlExpiryMinutes))
            };

            return await Task.FromResult(_s3Client.GetPreSignedURL(request));
        }

        public async Task<bool> DeleteFileAsync(string key)
        {
            try
            {
                await _s3Client.DeleteObjectAsync(_bucketName, key);
                _logger.LogInformation("Deleted S3 file: {Key}", key);
                return true;
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex, "Failed to delete S3 file: {Key}", key);
                return false;
            }
        }

        public async Task<bool> FileExistsAsync(string key)
        {
            try
            {
                await _s3Client.GetObjectMetadataAsync(_bucketName, key);
                return true;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
        }

        public async Task<string> CopyFileAsync(string sourceKey, string destinationKey)
        {
            var request = new CopyObjectRequest
            {
                SourceBucket = _bucketName,
                SourceKey = sourceKey,
                DestinationBucket = _bucketName,
                DestinationKey = destinationKey,
                ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
            };

            await _s3Client.CopyObjectAsync(request);
            _logger.LogInformation("Copied S3 file from {Source} to {Dest}", sourceKey, destinationKey);
            return destinationKey;
        }
    }
}
