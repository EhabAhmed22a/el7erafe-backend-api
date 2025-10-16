using ServiceAbstraction;
using Shared.DataTransferObject.TechnicianIdentityDTOs;

namespace el7erafe.Web.Services
{
    public class TechnicianFileService(IWebHostEnvironment _environment) : ITechnicianFileService
    {

        public async Task<TechRegisterToReturnDTO> ProcessTechnicianFilesAsync(TechRegisterDTO techRegisterDTO)
        {
            // Save files and get paths
            var nationalIdFrontPath = await SaveFileAsync(techRegisterDTO.NationalIdFront, "nationalid_front");
            var nationalIdBackPath = await SaveFileAsync(techRegisterDTO.NationalIdBack, "nationalid_back");
            var criminalRecordPath = await SaveFileAsync(techRegisterDTO.CriminalRecord, "criminal_record");

            // Return the processed DTO with paths
            return new TechRegisterToReturnDTO
            {
                Name = techRegisterDTO.Name,
                PhoneNumber = techRegisterDTO.PhoneNumber,
                Password = techRegisterDTO.Password,
                NationalId = techRegisterDTO.NationalId,
                NationalIdFrontPath = nationalIdFrontPath,
                NationalIdBackPath = nationalIdBackPath,
                CriminalRecordPath = criminalRecordPath,
                ServiceType = techRegisterDTO.ServiceType
            };
        }

        private async Task<string> SaveFileAsync(IFormFile file, string fileType)
        {
            // File validation is already done by ValidateFileAttribute
            var fileExtension = Path.GetExtension(file.FileName);
            var fileName = $"{fileType}_{Guid.NewGuid()}{fileExtension}";

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "Technician");

            // Only create directory if it doesn't exist
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/images/Technician/{fileName}";
        }
    }
}
