using DomainLayer.Contracts;
using DomainLayer.Exceptions;
using DomainLayer.Models;
using DomainLayer.Models.IdentityModule;
using DomainLayer.Models.IdentityModule.Enums;
using Microsoft.AspNetCore.Identity;
using Service.Helpers;
using ServiceAbstraction;
using Shared.DataTransferObject.Calendar;
using Shared.DataTransferObject.OffersDTOs;
using Shared.DataTransferObject.OtpDTOs;
using Shared.DataTransferObject.ServiceRequestDTOs;
using Shared.DataTransferObject.TechnicianIdentityDTOs;
using Shared.DataTransferObject.UpdateDTOs;
using System;

namespace Service
{
    public class TechnicianFlowService(ITechnicianRepository technicianRepository, IBlobStorageRepository blobStorageRepository,
        IUnitOfWork unitOfWork,
        IIgnoredServiceRequestsRepository ignoredServiceRequestsRepository,
        OtpHelper otpHelper,
        UserManager<ApplicationUser> userManager,
        IServiceRequestRepository serviceRequestRepository,
        IOffersRepository offersRepository,
        IReservationRepository reservationRepository) : ITechnicianFlowService
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

            var setPhoneResult = await userManager.SetPhoneNumberAsync(user.User, updatePhoneDTO.PhoneNumber);
            var setNameResult = await userManager.SetUserNameAsync(user.User, updatePhoneDTO.PhoneNumber);

            if (!setPhoneResult.Succeeded || !setNameResult.Succeeded)
                throw new TechnicalException();
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
                var updateResult = await userManager.UpdateAsync(user.User);

                if (!updateResult.Succeeded)
                    throw new TechnicalException();
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
                throw new InvalidOtpException();

            await unitOfWork.BeginTransactionAsync();
            try
            {
                if (await technicianRepository.EmailExistsAsync(user.User.PendingEmail))
                {
                    await unitOfWork.RollbackTransactionAsync();
                    throw new UnprocessableEntityException("البريد الإلكتروني أصبح مستخدم بالفعل");
                }

                await userManager.SetEmailAsync(user.User, user.User.PendingEmail);

                user.User.PendingEmail = null;
                user.User.EmailConfirmed = true;

                var updateResult = await userManager.UpdateAsync(user.User);

                if (!updateResult.Succeeded)
                    throw new TechnicalException();

                await unitOfWork.CommitTransactionAsync();
            }
            catch (UnprocessableEntityException)
            {
                throw;
            }
            catch (Exception)
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

            // await CheckReservations() - will be added later

            await unitOfWork.BeginTransactionAsync();

            try
            {
                var serviceRequestIds = await serviceRequestRepository.GetServiceRequestIdsByTechnicianAsync(technician.Id);

                if (!await serviceRequestRepository.DeleteServiceRequestsByTechnicianIdAsync(technician.Id))
                    throw new TechnicalException();

                if (!await ignoredServiceRequestsRepository.DeleteAllByTechnicianId(technician.Id))
                    throw new TechnicalException();

                int deleted = await technicianRepository.DeleteAsync(userId);
                if (deleted == 0)
                    throw new TechnicalException();

                await unitOfWork.CommitTransactionAsync();

                var blobDeleteTasks = new List<Task>();

                foreach (var srId in serviceRequestIds)
                {
                    blobDeleteTasks.Add(blobStorageRepository.DeleteBlobsWithPrefixAsync("service-requests-images", $"{srId}_"));
                }

                blobDeleteTasks.Add(blobStorageRepository.DeleteFileAsync(technician.CriminalHistoryURL, "technician-documents"));
                blobDeleteTasks.Add(blobStorageRepository.DeleteFileAsync(technician.NationalIdBackURL, "technician-documents"));
                blobDeleteTasks.Add(blobStorageRepository.DeleteFileAsync(technician.NationalIdFrontURL, "technician-documents"));
                blobDeleteTasks.Add(blobStorageRepository.DeleteFileAsync(technician.ProfilePictureURL, "technician-documents"));

                blobDeleteTasks.Add(blobStorageRepository.DeleteBlobsWithPrefixAsync("technician-documents", $"portifolioImages_{technician.Id}_"));

                await Task.WhenAll(blobDeleteTasks);
            }
            catch
            {
                await unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<List<BroadCastServiceRequestDTO>> GetAvailableRequests(string userId)
        {
            var user = await CheckUser(userId);

            var availableRequests = await serviceRequestRepository.GetAvailableServiceRequestsByTechnicianAsync(user.Id, user.ServiceId, user.City.GovernorateId);

            var mappingTasks = availableRequests.Select(async req =>
            {
                List<string> serviceURLs = await blobStorageRepository.GetBlobUrlsWithPrefixAsync("service-requests-images", $"{req.Id}_");

                string? clientImageURL = null;
                if (!string.IsNullOrEmpty(req.Client?.ImageURL))
                {
                    clientImageURL = await blobStorageRepository.GetBlobUrlWithSasTokenAsync("client-profilepics", req.Client.ImageURL);
                }

                return new BroadCastServiceRequestDTO()
                {
                    requestId = req.Id,
                    clientName = req.Client?.Name,
                    clientImage = clientImageURL,
                    day = req.ServiceDate,
                    clientTimeInterval = HelperClass.FormatArabicTimeInterval(req.AvailableFrom, req.AvailableTo),
                    serviceType = req.Service?.NameAr,
                    description = req.Description,
                    serviceImages = serviceURLs,
                    governorate = req.City?.Governorate?.NameAr,
                    city = req.City?.NameAr,
                    street = req.Street,
                    specialSign = req.SpecialSign,
                    From = req.AvailableFrom,
                    To = req.AvailableTo,
                    GovernorateId = req.City?.GovernorateId ?? 0,
                    ServiceId = req.ServiceId
                };
            });

            return (await Task.WhenAll(mappingTasks)).ToList();
        }

        public async Task<string?> DeclineRequestAsync(string userId, ReqIdDTO cancelReqDTO)
        {
            var user = await CheckUser(userId);

            var service = await serviceRequestRepository.GetServiceById(cancelReqDTO.requestId);
            if (service is null)
                throw new TechnicalException();

            var clientUserId = service.Client.UserId;

            if (service.TechnicianId == user.Id)
            {
                try
                {
                    service.Status = ServiceReqStatus.Rejected;
                    if (!await serviceRequestRepository.UpdateAsync(service))
                        throw new TechnicalException();
                    return clientUserId;
                }
                catch
                {
                    throw new TechnicalException();
                }
            }
            else
            {
                try
                {
                    var tech = await technicianRepository.GetByUserIdAsync(userId);
                    if (tech is null)
                        throw new TechnicalException();
                    if (await ignoredServiceRequestsRepository.IsAlreadyIgnoredAsync(tech.Id, service.Id))
                        throw new RequestAlreadyDeclinedException();
                    await ignoredServiceRequestsRepository.CreateAsync(new IgnoredServiceRequest { TechnicianId = tech.Id, ServiceRequestId = service.Id });
                    return string.Empty;
                }
                catch(RequestAlreadyDeclinedException)
                {
                    throw;
                }
                catch (Exception)
                {
                    throw new TechnicalException();
                }
            }
            
        }

        public async Task<List<PendingOfferDto>> GetPendingOffersAsync(string technicianUserId)
        {
            var technician = await technicianRepository.GetByUserIdAsync(technicianUserId);

            if (technician == null)
                throw new TechNotFoundException(technicianUserId);

            var offers = await offersRepository.GetPendingOffersForTechAsync(technician.Id);

            var result = new List<PendingOfferDto>();

            foreach (var offer in offers)
            {
                List<string> serviceURLs = await blobStorageRepository.GetBlobUrlsWithPrefixAsync("service-requests-images", $"{offer.Id}_");
                result.Add(new PendingOfferDto
                {
                    OfferId = offer.Id,
                    Description = offer.ServiceRequest.Description,
                    ClientName = offer.ServiceRequest.Client.Name,
                    ClientImage = await blobStorageRepository.GetBlobUrlWithSasTokenAsync(
                        "client-profilepics",
                        offer.ServiceRequest.Client.ImageURL),

                    Fees = offer.Fees,
                    ServiceType = offer.ServiceRequest.Service.NameAr,

                    TechTimeInterval = HelperClass.FormatArabicTimeInterval(offer.WorkFrom, offer.WorkTo),

                    Day = offer.ServiceRequest.ServiceDate,

                    EndDay = offer.NumberOfDays != null
                        ? offer.ServiceRequest.ServiceDate.AddDays(offer.NumberOfDays.Value - 1)
                        : null,

                    Governorate = offer.ServiceRequest.City.Governorate.NameAr,
                    City = offer.ServiceRequest.City.NameAr,
                    Street = offer.ServiceRequest.Street,
                    ServiceImages = serviceURLs,
                    SpecialSign = offer.ServiceRequest.SpecialSign
                });
            }

            return result;
        }

        public async Task<List<TechnicianCalendarDto>> GetCalendar(string userId, DateTime? date)
        {
            var user = await CheckUser(userId);

            var targetDate = date ?? DateTime.UtcNow;

            var reservations = await reservationRepository.GetCurrentReservationsAsync(user.Id, targetDate);

            reservations = reservations
                .OrderBy(r => r.Offer.WorkFrom)
                .ToList();

            var tasks = reservations.Select(async reservation =>
            {
                var serviceURLsTask = blobStorageRepository
                    .GetBlobUrlsWithPrefixAsync("service-requests-images", $"{reservation.Offer.Id}_");

                var clientImageTask = blobStorageRepository
                    .GetBlobUrlWithSasTokenAsync(
                        "client-profilepics",
                        reservation.Offer.ServiceRequest.Client?.ImageURL);

                await Task.WhenAll(serviceURLsTask, clientImageTask);

                return new TechnicianCalendarDto
                {
                    ReservationId = reservation.Id,

                    Description = reservation.Offer.ServiceRequest.Description,

                    ClientName = reservation.Offer.ServiceRequest.Client?.Name,
                    ClientImage = clientImageTask.Result,

                    TechTimeInterval = HelperClass.FormatArabicTimeInterval(
                        reservation.Offer.WorkFrom,
                        reservation.Offer.WorkTo),

                    Day = reservation.Offer.ServiceRequest.ServiceDate,

                    Governorate = reservation.Offer.ServiceRequest.City?.Governorate?.NameAr,
                    City = reservation.Offer.ServiceRequest.City?.NameAr,
                    Street = reservation.Offer.ServiceRequest.Street,

                    ServiceImages = serviceURLsTask.Result,

                    SpecialSign = reservation.Offer.ServiceRequest.SpecialSign
                };
            });

            return (await Task.WhenAll(tasks)).ToList();
        }

        public async Task<Technician?> GetTechnicianByIdAsync(int techId)
        {
            try
            {
                return await technicianRepository.GetByIdAsync(techId);
            }
            catch
            {
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

