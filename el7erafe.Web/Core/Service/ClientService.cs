using DomainLayer.Contracts;
using DomainLayer.Exceptions;
using DomainLayer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ServiceAbstraction;
using Shared.DataTransferObject.ClientDTOs;
using Shared.DataTransferObject.ClientIdentityDTOs;
using Shared.DataTransferObject.ServiceRequestDTOs;
using Shared.DataTransferObject.UpdateDTOs;

namespace Service
{
    public class ClientService(ITechnicianServicesRepository technicianServicesRepository,
            IClientRepository clientRepository,
            IBlobStorageRepository blobStorageRepository,
            IServiceRequestRepository serviceRequestRepository,
            ITechnicianServicesRepository servicesRepository,
            ICityRepository cityRepository) : IClientService
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

        public async Task QuickReserve(ServiceRequestRegDTO regDTO, string userId)
        {
            if (regDTO.AllDayAvailability && (regDTO.AvailableFrom.HasValue || regDTO.AvailableTo.HasValue))
                throw new UnprocessableEntityException("عند اختيار 'متاح طوال اليوم'، يجب إرسال حقلي وقت البداية والنهاية فارغين");

            if (!regDTO.AllDayAvailability && (!regDTO.AvailableFrom.HasValue || !regDTO.AvailableTo.HasValue))
                throw new UnprocessableEntityException("يجب تحديد وقت البداية والنهاية عندما لا تكون متاحاً طوال اليوم");

            if (!regDTO.AllDayAvailability && regDTO.AvailableFrom >= regDTO.AvailableTo)
                throw new UnprocessableEntityException("وقت البداية يجب أن يكون قبل وقت النهاية");

            var client = await clientRepository.GetByUserIdAsync(userId);
            if (client is null)
                throw new ForbiddenAccessException("هذا الإجراء متاح للعملاء فقط");

            var city = await cityRepository.GetCityByNameAsync(regDTO.CityName);

            if (city is null)
                throw new CityNotFoundException(regDTO.CityName);

            if (!await servicesRepository.ExistsAsync((int)regDTO.ServiceId))
                throw new TechnicalException();

            int clientId = client.Id;
            if (await serviceRequestRepository.IsServiceAlreadyReq(clientId, regDTO.ServiceId))
                throw new ServiceAlreadyRequestedException();

            //******Still some Logic to be added after Implementing Reservations in the application******//

            var serviceReq = new ServiceRequest()
            {
                Description = regDTO!.Description,
                CityId = city.Id,
                ServiceId = (int)regDTO!.ServiceId,
                SpecialSign = regDTO!.SpecialSign,
                Street = regDTO!.Street,
                ServiceDate = (DateOnly)regDTO!.ServiceDate,
                AvailableFrom = regDTO!.AvailableFrom,
                AvailableTo = regDTO!.AvailableTo,
                CreatedAt = DateTime.UtcNow,
                ClientId = clientId
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
                user.ImageURL = await blobStorageRepository.UploadFileAsync(dTO.Image!, "client-profilepics", $"{user.Id}{Path.GetExtension(dTO.Image?.FileName)}");

            await clientRepository.UpdateAsync(user);
        }
    }
}
