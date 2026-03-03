
using Azure;
using DomainLayer.Contracts;
using DomainLayer.Exceptions;
using DomainLayer.Models.IdentityModule;
using ServiceAbstraction;
using Shared.DataTransferObject.ClientIdentityDTOs;
using Shared.DataTransferObject.TechnicianIdentityDTOs;

namespace Service
{
    public class TechnicianFlowService(ITechnicianRepository technicianRepository, IBlobStorageRepository blobStorageRepository,
        IUnitOfWork unitOfWork) : ITechnicianService
    {
        public async Task<TechnicianProfileDTO> GetProfile(string userId)
        {
            var user = await CheckUser(userId);

            var isImageNull = user.ProfilePictureURL is not null;

            return new TechnicianProfileDTO()
            {
                Name = user.Name,
                Email = user.User.Email!,
                ProfileImage = isImageNull ? await blobStorageRepository.GetBlobUrlWithSasTokenAsync("technician-documents", user.ProfilePictureURL!) : "https://el7erafe.blob.core.windows.net/services-documents/user-circles-set.png",
                Phone = user.User.PhoneNumber!,
                AboutMe = user.AboutMe,
                PortifolioImages = await blobStorageRepository.GetBlobUrlsWithPrefixAsync("technician-documents", $"portifolioImages_{user.Id}_")
            };
        }

        public async Task UpdateBasicInfo(string userId, UpdateTechnicianDTO updateTechnicianDTO)
        {
            if (IsEmptyUpdateRequest(updateTechnicianDTO))
                throw new ArgumentException("لم يتم إرسال أي بيانات للتحديث. الرجاء تعديل حقل واحد على الأقل");

            var user = await CheckUser(userId);
            bool textFieldsUpdated = false;

            if (updateTechnicianDTO.Name is not null)
            {
                if (user.Name.Equals(updateTechnicianDTO.Name))
                    throw new UpdateException("الاسم الجديد مطابق للاسم الحالي");
                user.Name = updateTechnicianDTO.Name;
                textFieldsUpdated = true;
            }

            if (updateTechnicianDTO.AboutMe is not null)
            {
                if (user.AboutMe is not null && user.AboutMe.Equals(updateTechnicianDTO.AboutMe))
                    throw new UpdateException("الوصف الجديد مطابق للوصف الحالي");
                user.AboutMe = updateTechnicianDTO.AboutMe;
                textFieldsUpdated = true;
            }

            if (updateTechnicianDTO.ProfileImage is not null && updateTechnicianDTO.ProfileImage.Length > 0)
            {
                await unitOfWork.BeginTransactionAsync();
                try
                {
                    string? oldImageUrl = user.ProfilePictureURL;
                    user.ProfilePictureURL = await blobStorageRepository.UploadFileAsync(
                        updateTechnicianDTO.ProfileImage,
                        "technician-documents");
                    if (await technicianRepository.UpdateAsync(user) != 1)
                    {
                        await unitOfWork.RollbackTransactionAsync();
                        throw new TechnicalException();
                    }

                    if (!string.IsNullOrEmpty(oldImageUrl))
                    {
                        await blobStorageRepository.DeleteFileAsync(oldImageUrl, "technician-documents");
                    }

                    await unitOfWork.CommitTransactionAsync();
                }
                catch
                {
                    await unitOfWork.RollbackTransactionAsync();
                    throw new TechnicalException();

                }
            }

            if (updateTechnicianDTO.DeletedPortifolioImages is not null && updateTechnicianDTO.DeletedPortifolioImages.Count > 0)
            {
                foreach (var fileName in updateTechnicianDTO.DeletedPortifolioImages)
                {
                    bool exists = await blobStorageRepository.FileExistsAsync(fileName, "technician-documents");
                    if (!exists)
                    {
                        throw new TechnicalException();
                    }
                }
                foreach (var fileName in updateTechnicianDTO.DeletedPortifolioImages)
                {
                    var fileExtnesion = Path.GetExtension(fileName);
                    try
                    {
                        await blobStorageRepository.DeleteFileAsync(fileName, "technician-documents");
                    }
                    catch
                    {
                        throw new TechnicalException();
                    }
                }
            }

            if (updateTechnicianDTO.NewPortifolioImages is not null && updateTechnicianDTO.NewPortifolioImages.Count > 0)
            {
                int count = await blobStorageRepository.CountBlobsWithPrefixAsync("technician-documents", $"portifolioImages_{user.Id}_");
                if (count >= 20)
                    throw new UnprocessableEntityException($"لديك بالفعل {count} صور. لا يمكن إضافة المزيد. الرجاء حذف بعض الصور أولاً ثم حاول الإضافة");

                int newTotal = count + updateTechnicianDTO.NewPortifolioImages.Count;
                if (newTotal > 20)
                    throw new UnprocessableEntityException($"لا يمكن إضافة {updateTechnicianDTO.NewPortifolioImages.Count} صور لأن المجموع سيكون {newTotal} والحد الأقصى 20. الرجاء حذف {newTotal - 20} صور أولاً");

                try
                {
                    await blobStorageRepository.UploadMultipleFilesAsync(
                        updateTechnicianDTO.NewPortifolioImages,
                        "technician-documents",
                        $"portifolioImages_{user.Id}_{Guid.NewGuid()}"
                    );
                }
                catch
                {
                    throw new TechnicalException();
                }
            }
            if (textFieldsUpdated &&
            updateTechnicianDTO.ProfileImage is null &&
            updateTechnicianDTO.NewPortifolioImages?.Count == 0 &&
            updateTechnicianDTO.DeletedPortifolioImages?.Count == 0)
            {
                if (await technicianRepository.UpdateAsync(user) != 1)
                    throw new TechnicalException();
            }
        }


        private async Task<Technician> CheckUser(string userId)
        {
            var user = await technicianRepository.GetByUserIdAsync(userId);
            if (user is null)
                throw new UserNotFoundException("المستخدم غير موجود");
            return user;
        }

        private bool IsEmptyUpdateRequest(UpdateTechnicianDTO dTO)
        {
            return string.IsNullOrWhiteSpace(dTO.AboutMe) &&
                   string.IsNullOrWhiteSpace(dTO.Name) &&
                   dTO.ProfileImage == null &&
                   (dTO.NewPortifolioImages == null || dTO.NewPortifolioImages.Count == 0) &&
                   (dTO.DeletedPortifolioImages == null || dTO.DeletedPortifolioImages.Count == 0);
        }
    }
}

