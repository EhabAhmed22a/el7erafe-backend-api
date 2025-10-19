using ServiceAbstraction;
using Shared.DataTransferObject.TechnicianIdentityDTOs;
using static DomainLayer.Contracts.IBlobStorage;

namespace Service
{
    public class TechnicianFileService : ITechnicianFileService
    {
        private readonly IBlobStorageService _blobStorageService;

        public TechnicianFileService(IBlobStorageService blobStorageService)
        {
            _blobStorageService = blobStorageService;
        }

        public async Task<TechRegisterToReturnDTO> ProcessTechnicianFilesAsync(TechRegisterDTO techRegisterDTO)
        {
            // Save files to blob storage and get URLs
            var nationalIdFrontUrl = await _blobStorageService.UploadFileAsync(
                techRegisterDTO.NationalIdFront,
                "technician-documents",
                $"nationalid_front_{Guid.NewGuid()}"
            );

            var nationalIdBackUrl = await _blobStorageService.UploadFileAsync(
                techRegisterDTO.NationalIdBack,
                "technician-documents",
                $"nationalid_back_{Guid.NewGuid()}"
            );

            var criminalRecordUrl = await _blobStorageService.UploadFileAsync(
                techRegisterDTO.CriminalRecord,
                "technician-documents",
                $"criminal_record_{Guid.NewGuid()}"
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
    }
}