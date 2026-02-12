using DomainLayer.Contracts;
using DomainLayer.Exceptions;
using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;
using ServiceAbstraction;
using Shared.DataTransferObject.ClientDTOs;
using Shared.DataTransferObject.ServiceRequestDTOs;

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

            if (!await servicesRepository.ExistsAsync((int)regDTO.ServiceId) || !await cityRepository.ExistsAsync((int)regDTO.CityId))
                throw new TechnicalException();

            int clientId = client.Id;
            if (await serviceRequestRepository.IsServiceAlreadyReq(clientId, regDTO.ServiceId))
                throw new ServiceAlreadyRequestedException();

            //******Still some Logic to be added after Implementing Reservations in the application******//

            var serviceReq = new ServiceRequest()
            {
                Description = regDTO!.Description,
                CityId = (int) regDTO!.CityId,
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
    }
}
