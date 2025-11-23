// LookupService.cs
using DomainLayer.Contracts;
using DomainLayer.Models.IdentityModule;
using Microsoft.Extensions.Logging;
using ServiceAbstraction;
using Shared.DataTransferObject.LookupDTOs;

namespace Service
{
    public class LookupService(ITechnicianRepository _technicianRepository,
                ILogger<LookupService> _logger) : ILookupService
    {
        async Task<ServicesDto> ILookupService.GetAllServicesAsync()
        {
            try
            {
                _logger.LogInformation("[SERVICE] Retrieving all services");
                var services = await _technicianRepository.GetAllServicesAsync();
                if (services == null || !services.Any())
                {
                    _logger.LogWarning("[SERVICE] No services found");
                    return new ServicesDto { Services = new List<string>() };
                }

                // Return Arabic names wrapped in ServicesResponseDto
                var serviceNames = services.Select(s => s.NameAr).ToList();
                return new ServicesDto { Services = serviceNames };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SERVICE] Error retrieving all services");
                throw;
            }
        }
    }
}