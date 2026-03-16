using System.Collections.Generic;
using DomainLayer.Contracts;
using DomainLayer.Exceptions;
using DomainLayer.Models;
using DomainLayer.Models.IdentityModule;
using DomainLayer.Models.IdentityModule.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Service.Helpers;
using ServiceAbstraction;
using Shared.DataTransferObject.ClientDTOs;
using Shared.DataTransferObject.ClientIdentityDTOs;
using Shared.DataTransferObject.OffersDTOs;
using Shared.DataTransferObject.OtpDTOs;
using Shared.DataTransferObject.ServiceRequestDTOs;
using Shared.DataTransferObject.UpdateDTOs;

namespace Service
{
    public class ClientService(ITechnicianServicesRepository technicianServicesRepository,
            IClientRepository clientRepository,
            IBlobStorageRepository blobStorageRepository,
            UserManager<ApplicationUser> userManager,
            IServiceRequestRepository serviceRequestRepository,
            ITechnicianServicesRepository servicesRepository,
            ITechnicianRepository technicianRepository,
            ICityRepository cityRepository,
            OtpHelper otpHelper,
            IUnitOfWork unitOfWork,
            IOffersRepository offersRepository,
            IReservationRepository reservationRepository) : IClientService
    {
        public async Task<ServiceListDto> GetClientServicesAsync()
        {
            var services = await technicianServicesRepository.GetAllAsync();

            if (services is null || !services.Any())
                return new ServiceListDto();

            var result = new ServiceListDto
            {
                Services = services.Select(s => new ServicesDto
                {
                    Id = s.Id,
                    Name = s.NameAr,
                    ImageURL = s?.ServiceImageURL
                }).ToList()
            };

            return result;
        }

        public async Task<BroadCastServiceRequestDTO> ServiceRequest(ServiceRequestRegDTO regDTO, string userId)
        {
            if (regDTO.AllDayAvailability)
            {
                if (regDTO.AvailableFrom.HasValue || regDTO.AvailableTo.HasValue)
                    throw new UnprocessableEntityException("عند اختيار 'متاح طوال اليوم'، يجب إرسال حقلي وقت البداية والنهاية فارغين");
            }
            else
            {
                if (!regDTO.AvailableFrom.HasValue || !regDTO.AvailableTo.HasValue)
                    throw new UnprocessableEntityException("يجب تحديد وقت البداية والنهاية عندما لا تكون متاحاً طوال اليوم");

                var fromTime = regDTO.AvailableFrom.Value;
                var toTime = regDTO.AvailableTo.Value;

                if (fromTime == toTime)
                    throw new UnprocessableEntityException("وقت البداية والنهاية لا يمكن أن يكونا متطابقين");

                TimeSpan duration = toTime - fromTime;

                if (duration.TotalHours < 0)
                {
                    duration += TimeSpan.FromHours(24);
                }

                if (duration.TotalHours > 23)
                    throw new UnprocessableEntityException("إذا كنت متاحاً طوال اليوم، الرجاء اختيار 'متاح طوال اليوم'");

                if (regDTO.ServiceDate == DateOnly.FromDateTime(DateTime.Today))
                {
                    if (fromTime.ToTimeSpan() < DateTime.Now.TimeOfDay)
                        throw new UnprocessableEntityException("وقت البداية لا يمكن أن يكون في الماضي");
                }
            }

            var client = await clientRepository.GetByUserIdAsync(userId);
            if (client is null)
                throw new ForbiddenAccessException("هذا الإجراء متاح للعملاء فقط");

            var city = await cityRepository.GetCityByNameAsync(regDTO.CityName ?? "");
            if (city is null)
                throw new CityNotFoundException(regDTO.CityName ?? "");

            var service = await servicesRepository.GetByIdAsync(regDTO.ServiceId);
            if (service is null) throw new TechnicalException();

            if (regDTO.TechnicianId is not null)
            {
                var technician = await technicianRepository.GetByIdAsync((int)regDTO.TechnicianId);
                if (technician is null)
                    throw new UserNotFoundException("الفني المحدد غير موجود");

                var gov = await cityRepository.GetGovernateByCityId(city.Id);
                if (gov is null || technician.City.GovernorateId != gov.Id)
                    throw new TechnicalException();
            }

            int clientId = client.Id;
            if (await serviceRequestRepository.IsServicePending(clientId, regDTO.ServiceId))
                throw new ServiceAlreadyRequestedException();

            //******Still some Logic to be added after Implementing Reservations in the application******//

            var serviceReq = new ServiceRequest()
            {
                Description = regDTO.Description,
                CityId = city.Id,
                ServiceId = regDTO.ServiceId,
                SpecialSign = regDTO.SpecialSign,
                Street = regDTO.Street,
                ServiceDate = regDTO.ServiceDate,
                AvailableFrom = regDTO.AvailableFrom,
                AvailableTo = regDTO.AvailableTo,
                CreatedAt = DateTime.UtcNow,
                ClientId = clientId,
                TechnicianId = regDTO.TechnicianId,
                Status = ServiceReqStatus.Pending
            };
            ServiceRequest? createdRequest = null;
            bool noImages = !(regDTO.Images is not null && regDTO.Images.Count > 0);

            if (noImages)
            {
                try
                {
                    createdRequest = await serviceRequestRepository.CreateAsync(serviceReq);
                }
                catch
                {
                    throw new TechnicalException();
                }
            }
            else
            {
                List<string> uploadedFiles = new List<string>();
                await unitOfWork.BeginTransactionAsync();
                try
                {
                    createdRequest = await serviceRequestRepository.CreateAsync(serviceReq);

                    await blobStorageRepository.UploadMultipleFilesAsync(
                       regDTO.Images!,
                       "service-requests-images",
                       $"{createdRequest.Id}_{Guid.NewGuid()}");

                    await unitOfWork.CommitTransactionAsync();
                }
                catch
                {
                    await unitOfWork.RollbackTransactionAsync();

                    foreach (var file in uploadedFiles)
                    {
                        try
                        {
                            await blobStorageRepository.DeleteFileAsync(file, "service-requests-images");
                        }
                        catch { }
                    }

                    throw new TechnicalException();
                }
            }
            List<string> serviceURLs = new List<string>();
            if (!noImages)
            {
                serviceURLs = await blobStorageRepository.GetBlobUrlsWithPrefixAsync("service-requests-images", $"{createdRequest.Id}_");
            }

            string? clientImageURL = null;
            if (client.ImageURL is not null)
                clientImageURL = await blobStorageRepository.GetBlobUrlWithSasTokenAsync("client-profilepics", client.ImageURL);

            return new BroadCastServiceRequestDTO()
            {
                requestId = createdRequest.Id,
                clientName = client.Name,
                clientImage = clientImageURL,
                day = createdRequest.ServiceDate,
                clientTimeInterval = HelperClass.FormatArabicTimeInterval(createdRequest.AvailableFrom, createdRequest.AvailableTo),
                serviceType = service?.NameAr,
                description = createdRequest.Description,
                serviceImages = serviceURLs,
                governorate = city.Governorate?.NameAr,
                city = city.NameAr,
                street = createdRequest.Street,
                specialSign = createdRequest.SpecialSign,
                From = createdRequest.AvailableFrom,
                To = createdRequest.AvailableTo,
                GovernorateId = city.GovernorateId,
                ServiceId = createdRequest.ServiceId
            };
        }

        public async Task DeleteAccount(string userId)
        {
            var client = await clientRepository.GetByUserIdAsync(userId);
            if (client is null)
                throw new UserNotFoundException("المستخدم غير موجود");

            await unitOfWork.BeginTransactionAsync();

            try
            {
                // Get all service request ids
                var serviceRequestIds = await serviceRequestRepository.GetServiceRequestIdsByClientAsync(client.Id);

                // Delete ALL images for all service requests
                foreach (var srId in serviceRequestIds)
                {
                    await blobStorageRepository.DeleteBlobsWithPrefixAsync("service-requests-images", $"{srId}_");
                }

                // Delete client from database
                var deleted = await clientRepository.DeleteAsync(userId);
                if (!deleted)
                    throw new TechnicalException();

                await unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await unitOfWork.RollbackTransactionAsync();
                throw new TechnicalException();
            }
        }

        public async Task<ClientProfileDTO> GetProfileAsync(string userId)
        {
            var user = await clientRepository.GetByUserIdAsync(userId);
            if (user is null)
                throw new UserNotFoundException("المستخدم غير موجود");

            var isImageNull = user.ImageURL is not null;

            return new ClientProfileDTO()
            {
                Name = user.Name,
                Email = user.User.Email!,
                ImageURL = isImageNull ? await blobStorageRepository.GetBlobUrlWithSasTokenAsync("client-profilepics", user.ImageURL!) : "https://el7erafe.blob.core.windows.net/services-documents/user-circles-set.png",
                PhoneNumber = user.User.PhoneNumber!
            };
        }

        public async Task<List<AvailableTechnicianDto>> GetAvailableTechniciansAsync(GetAvailableTechniciansRequest requestRegDTO)
        {
            var service = await servicesRepository.GetServiceByNameAsync(requestRegDTO.ServiceName);
            if (service is null)
                throw new ServiceNotFoundException(requestRegDTO.ServiceName);

            var city = await cityRepository.GetCityByNameAsync(requestRegDTO.CityName);
            if (city is null)
                throw new CityNotFoundException(requestRegDTO.CityName);

            var governorate = await cityRepository.GetGovernateByCityId(city.Id);

            var technicians = await technicianRepository
                .GetTechniciansByServiceAndLocationAsync(service.Id, governorate.Id, city.Id, requestRegDTO.Sorted);

            if (technicians is null || !technicians.Any())
                return new List<AvailableTechnicianDto>();

            // FILTER BY AVAILABILITY
            //technicians = technicians
            //    .Where(t => t.Availability.Any(a =>
            //        (a.DayOfWeek == null || (int)a.DayOfWeek == requestRegDTO.DayOfWeek) &&
            //        a.FromTime <= requestRegDTO.FromTime &&
            //        a.ToTime >= requestRegDTO.ToTime))
            //    .ToList();

            if (!technicians.Any())
                return new List<AvailableTechnicianDto>();

            var sasUrls = await GenerateProfilePictureSasUrlsAsync(technicians);

            var technicianDtos = new List<AvailableTechnicianDto>();

            foreach (var t in technicians)
            {
                var portfolioImages = await blobStorageRepository
                    .GetBlobUrlsWithPrefixAsync("technician-documents", $"portifolioImages_{t.Id}_");

                technicianDtos.Add(new AvailableTechnicianDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    ServiceName = t.Service.NameAr,
                    Rating = t.Rating,
                    City = t.City.NameAr,
                    About = t.AboutMe ?? string.Empty,
                    ProfilePicture = sasUrls.ContainsKey(t.ProfilePictureURL)
                        ? sasUrls[t.ProfilePictureURL]
                        : string.Empty,
                    PortfolioImages = portfolioImages
                });
            }

            return technicianDtos;
        }

        public async Task UpdateNameAndImage(string userId, UpdateNameImageDTO dTO)
        {
            var user = await clientRepository.GetByUserIdAsync(userId);
            if (user is null)
                throw new UserNotFoundException("المستخدم غير موجود");

            bool hasName = !string.IsNullOrWhiteSpace(dTO.Name);
            bool hasValidImage = dTO.Image is not null && dTO.Image.Length > 0;

            if (!hasName && !hasValidImage)
                throw new ArgumentException("يجب توفير الاسم أو الصورة على الأقل للتحديث");

            bool sameName = user.Name.Equals(dTO.Name);
            if (sameName)
                throw new UpdateException("الاسم الجديد مطابق للاسم الحالي");

            if (hasName)
                user.Name = dTO.Name!;

            if (hasValidImage)
            {
                if (user.ImageURL != null)
                    await blobStorageRepository.DeleteFileAsync(user.ImageURL, "client-profilepics");

                user.ImageURL = await blobStorageRepository.UploadFileAsync(dTO.Image!, "client-profilepics", $"{user.Id}{Path.GetExtension(dTO.Image?.FileName)}");

            }
            try
            {
                if (!await clientRepository.UpdateAsync(user))
                    throw new TechnicalException();
            }
            catch
            {
                throw new TechnicalException();
            }
        }

        public async Task UpdatePhoneNumber(string userId, UpdatePhoneDTO dTO)
        {
            var user = await CheckUser(userId);

            if (user.User.PhoneNumber == dTO.PhoneNumber)
                throw new UpdateException("رقم الهاتف الجديد مطابق للرقم الحالي");

            if (await clientRepository.ExistsAsync(dTO.PhoneNumber))
                throw new UnprocessableEntityException("رقم الهاتف مستخدم بالفعل من قبل عميل آخر");

            var setPhoneResult = await userManager.SetPhoneNumberAsync(user.User, dTO.PhoneNumber);
            var setNameResult = await userManager.SetUserNameAsync(user.User, dTO.PhoneNumber);

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
            if (await clientRepository.EmailExistsAsync(updateEmailDTO.NewEmail))
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
                if (await clientRepository.EmailExistsAsync(user.User.PendingEmail))
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

        public async Task<List<ServiceRequestDTO>> GetPendingServiceRequestsAsync(string userId)
        {
            var user = await clientRepository.GetByUserIdAsync(userId);
            if (user is null)
                throw new UserNotFoundException("المستخدم غير موجود");
            try
            {
                var notReservedRequests = await serviceRequestRepository.GetPendingServiceRequestsByClientAsync(user.Id);

                var mappingTasks = notReservedRequests.Select(async sr =>
                {
                    // 1. Determine the request type
                    bool isQuick = sr.TechnicianId is null;

                    // 2. Resolve the targeted Technician's image (Only exists for Direct Requests)
                    string? imageUrl = null;
                    if (!string.IsNullOrWhiteSpace(sr.Technician?.ProfilePictureURL))
                    {
                        imageUrl = await blobStorageRepository.GetBlobUrlWithSasTokenAsync("technician-documents", sr.Technician.ProfilePictureURL);
                    }

                    // 3. The Logic Check: ONLY grab the single offer details if it's a Direct Request
                    var directOffer = isQuick ? null : sr.Offers?.FirstOrDefault();

                    return new ServiceRequestDTO
                    {
                        requestId = sr.Id,
                        isQuickReserve = isQuick,
                        day = sr.ServiceDate,
                        serviceType = sr.Service?.NameAr ?? "غير معروف",
                        clientTimeInterval = HelperClass.FormatArabicTimeInterval(sr.AvailableFrom, sr.AvailableTo),

                        // Tell the mobile app exactly how many offers exist
                        numberOfOffers = sr.Offers?.Count ?? null,

                        // The specific Technician info (Null for Quick Reserves)
                        techName = sr.Technician?.Name,
                        techImage = imageUrl,

                        // Offer Specifics (Null for Quick Reserves, Populated for Direct Requests)
                        offerId = directOffer?.Id,
                        fees = directOffer?.Fees,
                        techTimeInterval = HelperClass.FormatArabicTimeInterval(
                            directOffer?.WorkFrom,
                            directOffer?.WorkTo
                        ),
                        numberOfDays = directOffer?.NumberOfDays ?? 1
                    };
                });

                return (await Task.WhenAll(mappingTasks)).ToList();
            }
            catch
            {
                throw new TechnicalException();
            }
        }

        public async Task<List<OfferResultDto>> GetOffersAsync(string userId, int requestId, bool isQuick)
        {
            var client = await CheckUser(userId);

            try
            {
                var validOffers = await offersRepository.GetValidOffersForClientAsync(requestId, client.Id, isQuick);

                if (!validOffers.Any())
                    return new List<OfferResultDto>();

                return (await Task.WhenAll(validOffers.Select(MapOffer))).ToList();
            }
            catch
            {
                throw new TechnicalException();
            }
        }

        public async Task<string?> CancelRequestAsync(string userId, ReqIdDTO reqDTO)
        {
            var user = await CheckUser(userId);

            var service = await serviceRequestRepository.GetServiceById(reqDTO.requestId);
            if (service is null)
                throw new TechnicalException();

            if (service.Status != ServiceReqStatus.Pending)
                throw new RequestAlreadyCanceledException();

            string? techUserId = null;
            if (service.TechnicianId is not null)
                techUserId = service.Technician?.UserId;

            service.Status = ServiceReqStatus.Canceled;

            try
            {
                if (!await serviceRequestRepository.UpdateAsync(service))
                    throw new TechnicalException();
            }
            catch
            {
                throw new TechnicalException();
            }
            return techUserId;
        }

        public async Task<Client?> GetClientByIdAsync(int clientId)
        {
            return await clientRepository.GetByIdAsync(clientId);
        }

        public async Task AcceptOffer(int offerId)
        {
            var existing = await reservationRepository.GetByOfferIdAsync(offerId);

            if (existing != null)
                throw new Exception("Offer already accepted");

            var offer = await offersRepository.GetByIdAsync(offerId);

            if (offer == null)
                throw new Exception("Offer not found");

            var request = await serviceRequestRepository.GetServiceById(offer.ServiceRequestId);

            if (request == null)
                throw new Exception("Service request not found");

            if (request.Status != ServiceReqStatus.Pending)
                throw new Exception("Service request already reserved");

            // Update service request status
            request.Status = ServiceReqStatus.Reserved;

            // Create reservation
            var reservation = new Reservation
            {
                OfferId = offer.Id,
                Status = ReservationStatus.Confirmed
            };

            await reservationRepository.AddAsync(reservation);
            await reservationRepository.SaveChangesAsync();
        }

        private async Task<Client> CheckUser(string userId)
        {
            var user = await clientRepository.GetByUserIdAsync(userId);
            if (user is null)
                throw new UserNotFoundException("المستخدم غير موجود");
            return user;
        }
        private async Task<Dictionary<string, string>> GenerateProfilePictureSasUrlsAsync(IEnumerable<Technician> technicians)
        {
            var profilePictureNames = technicians
                .Where(t => t != null && !string.IsNullOrEmpty(t.ProfilePictureURL))
                .Select(t => t.ProfilePictureURL)
                .Distinct()
                .ToList();

            if (!profilePictureNames.Any())
                return new Dictionary<string, string>();

            try
            {
                return await blobStorageRepository.GetMultipleBlobsUrlWithSasTokenAsync(
                    "technician-documents",
                    profilePictureNames,
                    expiryHours: 1
                ) ?? new Dictionary<string, string>();
            }
            catch (Exception ex)
            {
                return new Dictionary<string, string>();
            }
        }
        private async Task<OfferResultDto> MapOffer(Offer offer)
        {
            string techImageUrl = string.Empty;

            if (!string.IsNullOrWhiteSpace(offer.Technician?.ProfilePictureURL))
            {
                techImageUrl = await blobStorageRepository.GetBlobUrlWithSasTokenAsync("technician-documents",offer.Technician.ProfilePictureURL);
            }

            return new OfferResultDto
            {
                OfferId = offer.Id,
                RequestId = offer.ServiceRequestId,

                TechName = offer.Technician?.Name ?? "فني",
                TechImage = techImageUrl,
                NumberOfSuccessJobs = 0,
                Rate = offer.Technician?.Rating ?? 0,

                ServiceType = offer.ServiceRequest?.Service?.NameAr ?? "غير معروف",
                Fees = offer.Fees,
                TechTimeInterval = HelperClass.FormatArabicTimeInterval(offer.WorkFrom, offer.WorkTo),
                Day = offer.ServiceRequest.ServiceDate,
                NumberOfDays = offer.NumberOfDays,
                Comments = null,

                ClientId = offer.ServiceRequest.ClientId
            };
        }

    }
}