using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DomainLayer.Contracts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ServiceAbstraction;
using Shared.DataTransferObject.TechnicianIdentityDTOs;
namespace Service
{
    public class TechnicianFileService : ITechnicianFileService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TechnicianFileService> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly IBlobStorageRepository _blobStorageService;


        public TechnicianFileService(
            IBlobStorageRepository blobStorageService,
            IConfiguration configuration,
            ILogger<TechnicianFileService> logger,
            IWebHostEnvironment environment
            )
        {
            _blobStorageService = blobStorageService;
            _configuration = configuration;
            _logger = logger;
            _environment = environment;
            _blobServiceClient = CreateBlobServiceClient();
        }

        private BlobServiceClient CreateBlobServiceClient()
        {
            try
            {
                if (_environment.IsDevelopment())
                {
                    // Use connection string for development
                    var connectionString = _configuration.GetConnectionString("AzureBlobStorage");
                    if (string.IsNullOrEmpty(connectionString))
                    {
                        throw new InvalidOperationException("AzureBlobStorage connection string is not configured for development environment.");
                    }

                    _logger.LogInformation("Using connection string for Blob Storage (Development)");
                    return new BlobServiceClient(connectionString);
                }
                else
                {
                    // Use Managed Identity for production
                    var storageAccountName = _configuration["AzureBlobStorage:AccountName"];
                    if (string.IsNullOrEmpty(storageAccountName))
                    {
                        throw new InvalidOperationException("AzureBlobStorage AccountName is not configured for production environment.");
                    }

                    var blobUri = new Uri($"https://{storageAccountName}.blob.core.windows.net");
                    _logger.LogInformation("Using Managed Identity for Blob Storage (Production)");
                    return new BlobServiceClient(blobUri, new DefaultAzureCredential());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create BlobServiceClient");
                throw;
            }
        }


        public async Task<TechRegisterToReturnDTO> ProcessTechnicianFilesAsync(TechRegisterDTO techRegisterDTO)
        {
            // Save files to blob storage and get URLs
            var nationalIdFrontUrl = await _blobStorageService.UploadFileAsync(
                techRegisterDTO.NationalIdFront,
                "technician-documents",
                $"nationalidfront{Guid.NewGuid()}"
            );

            var nationalIdBackUrl = await _blobStorageService.UploadFileAsync(
                techRegisterDTO.NationalIdBack,
                "technician-documents",
                $"nationalidback{Guid.NewGuid()}"
            );

            var criminalRecordUrl = await _blobStorageService.UploadFileAsync(
                techRegisterDTO.CriminalRecord,
                "technician-documents",
                $"criminalrecord{Guid.NewGuid()}"
            );

            // Return the processed DTO with blob URLs
            return new TechRegisterToReturnDTO
            {
                Name = techRegisterDTO.Name,
                PhoneNumber = techRegisterDTO.PhoneNumber,
                Password = techRegisterDTO.Password,
                NationalId = techRegisterDTO.NationalId,
                NationalIdFrontPath = nationalIdFrontUrl,
                NationalIdBackPath = nationalIdBackUrl,
                CriminalRecordPath = criminalRecordUrl,
                ServiceType = techRegisterDTO.ServiceType
            };
        }

        public async Task<Stream> GetFileStreamAsync(string blobName)
        {
            try
            {
                if (string.IsNullOrEmpty(blobName))
                    throw new ArgumentException("Blob name cannot be null or empty");

                var containerClient = _blobServiceClient.GetBlobContainerClient("technician-documents");
                var blobClient = containerClient.GetBlobClient(blobName);

                if (!await blobClient.ExistsAsync())
                    throw new FileNotFoundException($"File not found in blob storage: {blobName}");

                return await blobClient.OpenReadAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving file stream for blob: {BlobName}", blobName);
                throw;
            }
        }
        public async Task<BlobProperties> GetFilePropertiesAsync(string blobName)
        {
            try
            {
                if (string.IsNullOrEmpty(blobName))
                    throw new ArgumentException("Blob name cannot be null or empty");

                var containerClient = _blobServiceClient.GetBlobContainerClient("technician-documents");
                var blobClient = containerClient.GetBlobClient(blobName);

                if (!await blobClient.ExistsAsync())
                    throw new FileNotFoundException($"File not found in blob storage: {blobName}");

                return await blobClient.GetPropertiesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving file properties for blob: {BlobName}", blobName);
                throw;
            }
        }
    }
}
