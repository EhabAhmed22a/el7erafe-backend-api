using DomainLayer.Contracts;
using DomainLayer.Exceptions;
using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;
using ServiceAbstraction;
using Shared.DataTransferObject.ClientDTOs;
using Shared.DataTransferObject.ClientIdentityDTOs;
using Shared.DataTransferObject.ServiceRequestDTOs;

namespace Service
{
    public class ClientService(ITechnicianServicesRepository technicianServicesRepository,
            IClientRepository clientRepository,
            IBlobStorageRepository blobStorageRepository,
            IServiceRequestRepository serviceRequestRepository,
            ITechnicianServicesRepository servicesRepository,
            ITechnicianRepository technicianRepository,
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
                ServiceId = (int) regDTO!.ServiceId,
                SpecialSign = regDTO!.SpecialSign,
                Street = regDTO!.Street,
                ServiceDate = (DateOnly) regDTO!.ServiceDate,
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
                ImageURL = user.ImageURL,
                PhoneNumber = user.User.PhoneNumber!
            };
        }

        public async Task<List<AvailableTechnicianDto>> GetAvailableTechniciansAsync(ServiceRequestRegDTO requestRegDTO)
        {
            var city = await cityRepository.GetCityByNameAsync(requestRegDTO.CityName);
            if (city is null)
                throw new CityNotFoundException(requestRegDTO.CityName);

            var governorate = await cityRepository.GetGovernateByCityId(city.Id);

            var technicians = await technicianRepository
                .GetTechniciansByGovernorateWithCityPriorityAsync(governorate.Id, city.Id);

            if (technicians is null || !technicians.Any())
                return new List<AvailableTechnicianDto>();

            // Map to DTO
            var result = technicians.Select(t => new AvailableTechnicianDto
            {
                Id = t.Id,
                Name = t.Name,
                ServiceName = t.Service.NameAr,
                Rating = t.Rating,
                City = t.City.NameAr,
                ProfilePicture = t.ProfilePictureURL
            }).ToList();
            return result;
        }
    }
}