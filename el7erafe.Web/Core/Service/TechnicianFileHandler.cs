using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ServiceAbstraction;
using Shared.DataTransferObject.TechnicianIdentityDTOs;

namespace Service
{
    public class TechnicianFileService : ITechnicianFileService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IConfiguration _configuration;

        public TechnicianFileService(IConfiguration configuration)
        {
            _configuration = configuration;
            var connectionString = _configuration.GetConnectionString("AzureBlobStorage");
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        public async Task<TechRegisterToReturnDTO> ProcessTechnicianFilesAsync(TechRegisterDTO techRegisterDTO)
        {
            // Save files to blob storage and get URLs
            var nationalIdFrontUrl = await SaveFileToBlobAsync(techRegisterDTO.NationalIdFront, "nationalid_front");
            var nationalIdBackUrl = await SaveFileToBlobAsync(techRegisterDTO.NationalIdBack, "nationalid_back");
            var criminalRecordUrl = await SaveFileToBlobAsync(techRegisterDTO.CriminalRecord, "criminal_record");

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

        private async Task<string> SaveFileToBlobAsync(IFormFile file, string fileType)
        {
            // File validation is already done by ValidateFileAttribute
            var fileExtension = Path.GetExtension(file.FileName);
            var fileName = $"{fileType}_{Guid.NewGuid()}{fileExtension}";

            // Get container client for technician documents
            var containerClient = _blobServiceClient.GetBlobContainerClient("technician-documents");

            // Create container if it doesn't exist with public read access
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            // Get blob client
            var blobClient = containerClient.GetBlobClient(fileName);

            // Upload file to blob storage
            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = file.ContentType
                }
            });

            // Return the public URL of the blob
            return blobClient.Uri.ToString();
        }
    }
}