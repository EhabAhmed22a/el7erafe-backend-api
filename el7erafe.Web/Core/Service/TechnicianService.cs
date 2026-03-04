
using Azure;
using DomainLayer.Contracts;
using DomainLayer.Exceptions;
using DomainLayer.Models.IdentityModule;
using Service.Helpers;
using ServiceAbstraction;
using Shared.DataTransferObject.ClientIdentityDTOs;
using Shared.DataTransferObject.OtpDTOs;
using Shared.DataTransferObject.TechnicianIdentityDTOs;
using Shared.DataTransferObject.UpdateDTOs;

namespace Service
{
    public class TechnicianFlowService(ITechnicianRepository technicianRepository, IBlobStorageRepository blobStorageRepository,
        IUnitOfWork unitOfWork,
        OtpHelper otpHelper,
        IServiceRequestRepository serviceRequestRepository) : ITechnicianService
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
                await technicianRepository.UpdateAsync(user);
            }
        }

        public async Task UpdatePhoneNumber(string userId, UpdatePhoneDTO updatePhoneDTO)
        {
            var user = await CheckUser(userId);

            if (user.User.PhoneNumber == updatePhoneDTO.PhoneNumber)
                throw new UpdateException("رقم الهاتف الجديد مطابق للرقم الحالي");

            if (await technicianRepository.ExistsAsync(updatePhoneDTO.PhoneNumber))
                throw new UnprocessableEntityException("رقم الهاتف مستخدم بالفعل من قبل عميل آخر");

            user.User.PhoneNumber = updatePhoneDTO.PhoneNumber;
            user.User.UserName = updatePhoneDTO.PhoneNumber;
            try
            {
                await technicianRepository.UpdateAsync(user);
            }
            catch
            {
                throw new TechnicalException();
            }
        }

        public async Task<OtpResponseDTO> UpdatePendingEmail(string userId, UpdateEmailDTO updateEmailDTO)
        {
            var user = await CheckUser(userId);

            if (!user.User.EmailConfirmed)
                throw new UnprocessableEntityException("يجب تأكيد البريد الإلكتروني الحالي أولاً");
            if (user.User.Email == updateEmailDTO.NewEmail)
                throw new UpdateException("البريد الإلكتروني الجديد مطابق للبريد الحالي");
            if (await technicianRepository.EmailExistsAsync(updateEmailDTO.NewEmail))
                throw new UnprocessableEntityException("البريد الإلكتروني مستخدم بالفعل");

            user.User.PendingEmail = updateEmailDTO.NewEmail;

            try
            {
                await technicianRepository.UpdateAsync(user);
            }
            catch
            {
                throw new TechnicalException();
            }
            var identifier = otpHelper.GetOtpIdentifier(userId);
            if (!otpHelper.CanResendOtp(identifier).Result)
            {
                throw new OtpAlreadySent();
            }
            await otpHelper.SendOTP(user.User, updateEmailDTO.NewEmail);

            return new OtpResponseDTO
            {
                Message = "تم إرسال الرمز إلى بريدك الإلكتروني الجديد. يرجى التحقق لإكمال التحديث."
            };
        }

        public async Task UpdateEmailAsync(string userId, OtpCodeDTO otpCode)
        {
            var user = await CheckUser(userId);

            if (string.IsNullOrEmpty(user.User.PendingEmail))
                throw new UnprocessableEntityException("لا يوجد بريد إلكتروني معلق للتحديث");

            var identifier = otpHelper.GetOtpIdentifier(userId);
            var result = await otpHelper.VerifyOtp(identifier, otpCode.OtpCode);

            if (!result)
            {
                throw new InvalidOtpException();
            }

            await unitOfWork.BeginTransactionAsync();
            try
            {
                if (await technicianRepository.EmailExistsAsync(user.User.PendingEmail))
                {
                    await unitOfWork.RollbackTransactionAsync();
                    throw new UnprocessableEntityException("البريد الإلكتروني أصبح مستخدم بالفعل");
                }

                user.User.Email = user.User.PendingEmail;
                user.User.NormalizedEmail = user.User.Email.ToUpperInvariant();
                user.User.PendingEmail = null;
                user.User.EmailConfirmed = true;

                await technicianRepository.UpdateAsync(user);
                await unitOfWork.CommitTransactionAsync();
            }
            catch (UnprocessableEntityException)
            {
                throw;
            }
            catch(Exception)
            {
                await unitOfWork.RollbackTransactionAsync();
                throw new TechnicalException();
            }
        }

        public async Task<OtpResponseDTO> ResendOtpForPendingEmail(string userId)
        {
            var user = await CheckUser(userId);

            if (string.IsNullOrEmpty(user.User.PendingEmail))
            {
                throw new UnprocessableEntityException("لا يوجد بريد إلكتروني معلق");
            }

            var identifier = otpHelper.GetOtpIdentifier(user.User.Id);
            if (!await otpHelper.CanResendOtp(identifier))
            {
                throw new OtpAlreadySent();
            }

            await otpHelper.SendOTP(user.User, user.User.PendingEmail);

            return new OtpResponseDTO
            {
                Message = "تم إعادة إرسال الرمز إلى بريدك الإلكتروني الجديد."
            };
        }

        public async Task DeleteAccount(string userId)
        {
            var technician = await CheckUser(userId);

            //await CheckReservations() - will be added later

            await unitOfWork.BeginTransactionAsync();

            try
            {
                var serviceRequestIds = await serviceRequestRepository.GetServiceRequestIdsByTechnicianAsync(technician.Id);

                foreach (var srId in serviceRequestIds)
                {
                    await serviceRequestRepository.DeleteAsync(srId);
                }

                int deleted = await technicianRepository.DeleteAsync(userId);
                if (deleted == 0)
                    throw new TechnicalException();

                await unitOfWork.CommitTransactionAsync();


                foreach (var srId in serviceRequestIds)
                {
                    await blobStorageRepository.DeleteBlobsWithPrefixAsync("service-requests-images", $"{srId}_");
                }

                await blobStorageRepository.DeleteFileAsync(technician.CriminalHistoryURL, "technician-documents");
                await blobStorageRepository.DeleteFileAsync(technician.NationalIdBackURL, "technician-documents");
                await blobStorageRepository.DeleteFileAsync(technician.NationalIdFrontURL, "technician-documents");
                await blobStorageRepository.DeleteFileAsync(technician.ProfilePictureURL, "technician-documents");
                if (await blobStorageRepository.CountBlobsWithPrefixAsync("technician-documents", $"portifolioImages_{technician.Id}_") > 0)
                    await blobStorageRepository.DeleteBlobsWithPrefixAsync("technician-documents", $"portifolioImages_{technician.Id}_");
            }
            catch
            {
                await unitOfWork.RollbackTransactionAsync();
                throw;
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

