using Microsoft.AspNetCore.Http;
namespace DomainLayer.Contracts
{
    public interface IBlobStorageRepository
    {
        Task<string> UploadFileAsync(IFormFile file, string containerName, string? customFileName = null);
        Task<List<string>> UploadMultipleFilesAsync(List<IFormFile> files, string containerName, string? customFileNames = null);
        Task DeleteFileAsync(string fileName, string containerName);
        Task<bool> FileExistsAsync(string fileName, string containerName);
        Task<Stream> DownloadFileAsync(string fileName, string containerName);
        string ExtractFileNameFromUrl(string url);
        Task DeleteMultipleFilesAsync(string fileName, string containerName);
        Task<string?> GetImageURL(string containerName, string fileName);
        Task<string?> GetBlobUrlWithSasTokenAsync(string containerName, string fileName, int expiryHours = 1);
        Task<Dictionary<string, string>> GetMultipleBlobsUrlWithSasTokenAsync(string containerName, List<string> fileNames, int expiryHours = 1);
    }
}
