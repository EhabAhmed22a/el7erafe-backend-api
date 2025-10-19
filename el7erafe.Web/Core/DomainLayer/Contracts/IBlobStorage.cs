using Microsoft.AspNetCore.Http;
namespace DomainLayer.Contracts
{
    public interface IBlobStorage
    {
        public interface IBlobStorageService
        {
            Task<string> UploadFileAsync(IFormFile file, string containerName, string? customFileName = null);
            Task DeleteFileAsync(string fileName, string containerName);
            Task<bool> FileExistsAsync(string fileName, string containerName);
            Task<Stream> DownloadFileAsync(string fileName, string containerName);
        }
    }
}
