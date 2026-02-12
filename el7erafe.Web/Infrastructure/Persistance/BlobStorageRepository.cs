using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DomainLayer.Contracts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Persistance
{
    public class BlobStorageRepository : IBlobStorageRepository
    {
        private readonly BlobServiceClient _blobServiceClient;

        public BlobStorageRepository(IConfiguration configuration, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                var connectionString = configuration.GetConnectionString("AzureBlobStorage");
                _blobServiceClient = new BlobServiceClient(connectionString);
            }
            else
            {
                var accountName = configuration.GetValue<string>("AzureBlobStorage:AccountName");
                if (string.IsNullOrEmpty(accountName))
                {
                    throw new InvalidOperationException("AzureBlobStorage AccountName is not configured for production environment.");
                }
                var blobServiceUri = new Uri($"https://{accountName}.blob.core.windows.net");
                _blobServiceClient = new BlobServiceClient(blobServiceUri, new ManagedIdentityCredential());
            }
        }

        public async Task<string> UploadFileAsync(IFormFile file, string containerName, string? customFileName = null)
        {
            // File validation
            if (file == null || file.Length == 0)
                throw new ArgumentException("File cannot be empty");

            // Generate filename
            var fileExtension = Path.GetExtension(file.FileName);
            var fileName = customFileName ?? $"{Guid.NewGuid()}{fileExtension}";

            // Get container client
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            // Upload file1
            var blobClient = containerClient.GetBlobClient(fileName);

            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = file.ContentType
                }
            });

            return fileName;
        }

        public async Task<List<string>> UploadMultipleFilesAsync(List<IFormFile> files, string containerName, string? customFileNames = null)
        {
            if (files == null || files.Count == 0)
                throw new ArgumentException("No files provided");

            var uploadedFileNames = new List<string>();
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i];

                if (file == null || file.Length == 0)
                    continue;

                var fileExtension = Path.GetExtension(file.FileName);
                var fileName = customFileNames is not null
                    ? $"{customFileNames}_{i+1}{fileExtension}"
                    : $"{Guid.NewGuid()}{fileExtension}";

                var blobClient = containerClient.GetBlobClient(fileName);

                using var stream = file.OpenReadStream();
                await blobClient.UploadAsync(stream, new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = file.ContentType
                    }
                });

                uploadedFileNames.Add(fileName);
            }

            return uploadedFileNames;
        }

        public async Task DeleteFileAsync(string fileName, string containerName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(fileName);
            await blobClient.DeleteIfExistsAsync();
        }

        public async Task<bool> FileExistsAsync(string fileName, string containerName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(fileName);
            return await blobClient.ExistsAsync();
        }

        public async Task<Stream> DownloadFileAsync(string fileName, string containerName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(fileName);
            var response = await blobClient.DownloadAsync();
            return response.Value.Content;
        }

        public string ExtractFileNameFromUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("url cannot be null or empty", nameof(url));

            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                var path = uri.AbsolutePath.TrimEnd('/');
                var lastSegment = path.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
                return lastSegment ?? string.Empty;
            }

            var idx = url.LastIndexOf('/');
            return idx >= 0 ? url[(idx + 1)..] : url;
        }
    }
}
