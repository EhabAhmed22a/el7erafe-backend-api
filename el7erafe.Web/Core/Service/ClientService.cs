using DomainLayer.Contracts;
using DomainLayer.Exceptions;
using DomainLayer.Models;
using ServiceAbstraction;
using Shared.DataTransferObject.ClientDTOs;
using Shared.DataTransferObject.ServiceRequestDTOs;

namespace Service
{
    public class ClientService(ITechnicianServicesRepository technicianServicesRepository,
            IClientRepository clientRepository,
            IBlobStorageRepository blobStorageRepository,
            IServiceRequestRepository serviceRequestRepository,
            ICityRepository cityRepository,
            ITechnicianServicesRepository servicesRepository) : IClientService
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

        public async Task<ServiceReqDTO> QuickReserve(ServiceRequestRegDTO regDTO, string userId)
        {
            if (regDTO.AllDayAvailability && (regDTO.AvailableFrom.HasValue || regDTO.AvailableTo.HasValue))
                throw new UnprocessableEntityException("عند اختيار 'متاح طوال اليوم'، يجب إرسال حقلي وقت البداية والنهاية فارغين");

            if (!regDTO.AllDayAvailability && (!regDTO.AvailableFrom.HasValue || !regDTO.AvailableTo.HasValue))
                throw new UnprocessableEntityException("يجب تحديد وقت البداية والنهاية عندما لا تكون متاحاً طوال اليوم");

            if (!regDTO.AllDayAvailability && regDTO.AvailableFrom >= regDTO.AvailableTo)
                throw new UnprocessableEntityException("وقت البداية يجب أن يكون قبل وقت النهاية");

            var client = await clientRepository.GetByUserIdAsync(userId);
            if (client is null)
                throw new TechnicalException();

            int clientId = client.Id;
            if (await serviceRequestRepository.IsServiceAlreadyReq(clientId, regDTO.ServiceId))
                throw new ServiceAlreadyRequestedException();

            var allDayAvail = regDTO.AllDayAvailability is true;
            if (!allDayAvail)
            {
                if (!await serviceRequestRepository.IsTimeConflicted(clientId, regDTO.AvailableFrom, regDTO.AvailableTo))
                    throw new ServiceRequestTimeConflictException();
            }

            //******Still some Logic to be added after Implementing Reservations in the application******//

            string? lastImageURL = null;
            if (regDTO.Images is not null && regDTO.Images.Count > 0)
            {
                var fileNames = await blobStorageRepository.UploadMultipleFilesAsync(regDTO.Images, "service-request-images", $"{regDTO.ServiceId}_{clientId}");
                lastImageURL = fileNames.LastOrDefault();
            }

            var serviceReq = new ServiceRequest()
            {
                Description = regDTO!.Description,
                CityId = regDTO!.CityId,
                ServiceId = regDTO!.ServiceId,
                SpecialSign = regDTO!.SpecialSign,
                Street = regDTO!.Street,
                ServiceDate = regDTO!.ServiceDate,
                LastImageURL = lastImageURL,
                AvailableFrom = regDTO!.AvailableFrom,
                AvailableTo = regDTO!.AvailableTo,
                CreatedAt = DateTime.UtcNow,
                ClientId = clientId
            };

            var city = await cityRepository.GetCityNameById(regDTO.CityId);
            var governate = await cityRepository.GetGovernateByCityId(regDTO.CityId);
            var service = await servicesRepository.GetByIdAsync(regDTO.ServiceId);

            return new ServiceReqDTO()
            {
                Description = regDTO.Description,
                City = city?.NameAr,
                ServiceName = service?.NameAr,
                SpecialSign = regDTO.SpecialSign,
                Street = regDTO.Street,
                ServiceDate = regDTO.ServiceDate,
                ImageURL = lastImageURL,
                AvailableFrom = regDTO!.AvailableFrom,
                AvailableTo = regDTO!.AvailableTo,
                Governate = governate?.NameAr,
                AllDayAvailability = allDayAvail
                //,Images = 
            };
        }
    }
}
