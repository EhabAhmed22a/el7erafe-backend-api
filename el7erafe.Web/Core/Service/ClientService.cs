using DomainLayer.Contracts;
using ServiceAbstraction;
using Shared.DataTransferObject.ClientDTOs;

namespace Service
{
    public class ClientService(ITechnicianServicesRepository technicianServicesRepository) : IClientService
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
                    ImageURL = s.ServiceImageURL
                }).ToList()
            };

            return result;
        }
    }
}
