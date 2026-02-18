using DomainLayer.Contracts;
using DomainLayer.Exceptions;
using DomainLayer.Models;
using DomainLayer.Models.IdentityModule;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Service.Helpers;
using ServiceAbstraction;
using Shared.DataTransferObject.ClientDTOs;
using Shared.DataTransferObject.ClientIdentityDTOs;
using Shared.DataTransferObject.OtpDTOs;
using Shared.DataTransferObject.ServiceRequestDTOs;
using Shared.DataTransferObject.UpdateDTOs;

namespace Service
{
    public class ClientService(ITechnicianServicesRepository technicianServicesRepository,
            IClientRepository clientRepository,
            IUserTokenRepository userTokenRepository,
            IBlobStorageRepository blobStorageRepository,
            IServiceRequestRepository serviceRequestRepository,
            ITechnicianServicesRepository servicesRepository,
            ITechnicianRepository technicianRepository,
            ICityRepository cityRepository,
            ITechnicianRepository technicianRepository,
            OtpHelper otpHelper) : IClientService
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

        public async Task ServiceRequest(ServiceRequestRegDTO regDTO, string userId)
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

                if (regDTO.AvailableFrom.Value >= regDTO.AvailableTo.Value)
                    throw new UnprocessableEntityException("وقت البداية يجب أن يكون قبل وقت النهاية");

                if ((regDTO.AvailableTo.Value - regDTO.AvailableFrom.Value).TotalHours > 23)
                    throw new UnprocessableEntityException("إذا كنت متاحاً طوال اليوم، الرجاء اختيار 'متاح طوال اليوم'");
                
                if (regDTO.ServiceDate == DateOnly.FromDateTime(DateTime.Today))
                {
                    if (regDTO.AvailableFrom.Value.ToTimeSpan() < DateTime.Now.TimeOfDay)
                        throw new UnprocessableEntityException("وقت البداية لا يمكن أن يكون في الماضي");
                }
                
            }

            var client = await clientRepository.GetByUserIdAsync(userId);
            if (client is null)
                throw new ForbiddenAccessException("هذا الإجراء متاح للعملاء فقط");

            var city = await cityRepository.GetCityByNameAsync(regDTO.CityName ?? "");

            if (city is null)
                throw new CityNotFoundException(regDTO.CityName ?? "");

            if (!await servicesRepository.ExistsAsync(regDTO.ServiceId))
                throw new TechnicalException();

            if (regDTO.TechnicianId is not null)
            {
                if (await technicianRepository.GetByIdAsync((int)regDTO.TechnicianId) is null)
                    throw new UserNotFoundException("الفني المحدد غير موجود");
            }
                
            int clientId = client.Id;
            if (await serviceRequestRepository.IsServiceAlreadyReq(clientId, regDTO.ServiceId))
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
                CreatedAt = DateTime.Now,
                ClientId = clientId,
                TechnicianId = regDTO.TechnicianId
            };

            var serviceRequest = await serviceRequestRepository.CreateAsync(serviceReq);

            string? lastImageURL = null;
            if (regDTO.Images is not null && regDTO.Images.Count > 0)
            {
                var fileNames = await blobStorageRepository.UploadMultipleFilesAsync(regDTO.Images, "service-requests-images", $"{serviceRequest.Id}_{clientId}");
                lastImageURL = fileNames.LastOrDefault();
            }

            serviceRequest.LastImageURL = lastImageURL;
            if (!await serviceRequestRepository.UpdateAsync(serviceRequest))
                throw new TechnicalException();
        }

        public async Task DeleteAccount(string userId)
        {
            var client = await clientRepository.GetByUserIdAsync(userId);
            if (client is null)
                throw new UserNotFoundException("المستخدم غير موجود");

            // Get all service request ids for this client
            var serviceRequestIds = await serviceRequestRepository.GetServiceRequestIdsByClientAsync(client.Id);

            foreach (var srId in serviceRequestIds)
            {
                var sr = await serviceRequestRepository.GetServiceById(srId);
                if (sr is null)
                    continue;

                // If the service request has images, delete them from blob storage.
                if (!string.IsNullOrWhiteSpace(sr.LastImageURL))
                {
                    await blobStorageRepository.DeleteMultipleFilesAsync(sr.LastImageURL, "service-requests-images");
                }
            }

            var deleted = await clientRepository.DeleteAsync(userId);
        }

        public async Task<ClientProfileDTO> GetProfileAsync(string userId)
        {
            var user = await clientRepository.GetByUserIdAsync(userId);
            if (user is null)
                throw new UserNotFoundException("المستخدم غير موجود");

            return new ClientProfileDTO()
            {
                Name = user.Name,
                Email = user.User.Email!,
                ImageURL = user.ImageURL ?? "https://el7erafe.blob.core.windows.net/services-documents/user-circles-set.png",
                PhoneNumber = user.User.PhoneNumber!
            };
        }

        public async Task<List<AvailableTechnicianDto>> GetAvailableTechniciansAsync(GetAvailableTechniciansRequest requestRegDTO)
        {
            var city = await cityRepository.GetCityByNameAsync(requestRegDTO.CityName);
            if (city is null)
                throw new CityNotFoundException(requestRegDTO.CityName);

            var governorate = await cityRepository.GetGovernateByCityId(city.Id);

            var technicians = await technicianRepository
                .GetAvailableApprovedTechniciansWithSortingAsync(governorate.Id, city.Id, requestRegDTO.Sorted);

            if (technicians is null || !technicians.Any())
                return new List<AvailableTechnicianDto>();

            // Generate SAS URLs for all profile pictures
            var sasUrls = await GenerateProfilePictureSasUrlsAsync(technicians);

            // Map to DTOs
            var result = technicians.Select(t => new AvailableTechnicianDto
            {
                Id = t.Id,
                Name = t.Name,
                ServiceName = t.Service.NameAr,
                Rating = t.Rating,
                City = t.City.NameAr,
                ProfilePicture = sasUrls.ContainsKey(t.ProfilePictureURL) ? sasUrls[t.ProfilePictureURL] : string.Empty
            }).ToList();

            return result;
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

                user.ImageURL = await blobStorageRepository.GetImageURL("client-profilepics", await blobStorageRepository.UploadFileAsync(dTO.Image!, "client-profilepics", $"{user.Id}{Path.GetExtension(dTO.Image?.FileName)}"));

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
            var user = await clientRepository.GetByUserIdAsync(userId);
            if (user is null)
                throw new UserNotFoundException("المستخدم غير موجود");

            if (user.User.PhoneNumber == dTO.PhoneNumber)
                throw new UpdateException("رقم الهاتف الجديد مطابق للرقم الحالي");

            if (await clientRepository.ExistsAsync(dTO.PhoneNumber))
                throw new UnprocessableEntityException("رقم الهاتف مستخدم بالفعل من قبل عميل آخر");

            user.User.PhoneNumber = dTO.PhoneNumber;
            user.User.UserName = dTO.PhoneNumber;
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

        public async Task<OtpResponseDTO> UpdateEmail(string userId, UpdateEmailDTO updateEmailDTO)
        {
            var user = await CheckUser(userId);

            if (!user.User.EmailConfirmed)
                throw new UnprocessableEntityException("يجب تأكيد البريد الإلكتروني الحالي أولاً");
            if (user.User.Email == updateEmailDTO.NewEmail)
                throw new UpdateException("البريد الإلكتروني الجديد مطابق للبريد الحالي");
            if (await clientRepository.EmailExistsAsync(updateEmailDTO.NewEmail))
                throw new UnprocessableEntityException("البريد الإلكتروني مستخدم بالفعل");

            user.User.Email = updateEmailDTO.NewEmail;
            user.User.EmailConfirmed = false;
            user.User.NormalizedEmail = updateEmailDTO.NewEmail.ToUpperInvariant();

            try
            {
                if (!await clientRepository.UpdateAsync(user))
                    throw new TechnicalException();
            }
            catch
            {
                throw new TechnicalException();
            }

            await otpHelper.SendOTP(user.User);
            await userTokenRepository.DeleteUserTokenAsync(userId);

            return new OtpResponseDTO
            {
                Message = "تم إرسال الرمز إلى بريدك الإلكتروني الجديد. يرجى التحقق لإكمال التحديث."
            };
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
    }
}