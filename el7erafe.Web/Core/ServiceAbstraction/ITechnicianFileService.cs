using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Shared.DataTransferObject.TechnicianIdentityDTOs;

namespace ServiceAbstraction
{
    public interface ITechnicianFileService
    {
        Task<TechRegisterToReturnDTO> ProcessTechnicianFilesAsync(TechRegisterDTO techRegisterDTO);
        Task<Stream> GetFileStreamAsync(string blobName);
        Task<BlobProperties> GetFilePropertiesAsync(string blobName);
    }
}
