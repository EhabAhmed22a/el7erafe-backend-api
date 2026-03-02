
using DomainLayer.Contracts;
using DomainLayer.Exceptions;
using ServiceAbstraction;
using Shared.DataTransferObject.ClientIdentityDTOs;
using Shared.DataTransferObject.TechnicianIdentityDTOs;

namespace Service
{
    public class TechnicianFlowService(ITechnicianRepository technicianRepository, IBlobStorageRepository blobStorageRepository) : ITechnicianService
    {
        public async Task<TechnicianProfileDTO> GetProfile(string userId)
        {
            var user = await technicianRepository.GetByUserIdAsync(userId);
            if(user is null)
                throw new UserNotFoundException("المستخدم غير موجود");

            var isImageNull = user.ProfilePictureURL is not null;

            return new TechnicianProfileDTO()
            {
                Name = user.Name,
                Email = user.User.Email!,
                ProfileImage = isImageNull ? await blobStorageRepository.GetBlobUrlWithSasTokenAsync("technician-documents", user.ProfilePictureURL!) : "https://el7erafe.blob.core.windows.net/services-documents/user-circles-set.png",
                Phone = user.User.PhoneNumber!,
                AboutMe = user.AboutMe,
                PortifolioImages = new List<string>() // Add the method where it returns multiple photos based on sequence number
            };
        }
    }
}
