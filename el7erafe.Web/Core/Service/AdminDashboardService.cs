using DomainLayer.Contracts;
using DomainLayer.Exceptions;
using DomainLayer.Models.IdentityModule;
using Microsoft.Extensions.Logging;
using ServiceAbstraction;
using Shared.DataTransferObject.AdminDTOs.Dashboard;

namespace Service
{
    public class AdminDashboardService(IClientRepository clientRepository, ILogger<AdminDashboardService> logger) : IAdminDashboardService
    {
        public async Task<ClientListDTO> GetClientsAsync(int? pageNumber, int? pageSize)
        {
            pageNumber = (pageNumber.HasValue && pageNumber.Value < 1) ? 1 : pageNumber;
            pageSize = (pageSize.HasValue && pageSize.Value < 1) ? 10 : pageSize;

            logger.LogInformation("[SERVICE] GetClients method started. PageNumber: {PageNumber}, PageSize: {PageSize}",
                pageNumber, pageSize);

            try
            {
                logger.LogInformation("[SERVICE] Retrieving clients from repository. Using pagination: {UsePagination}",
                    pageNumber.HasValue && pageSize.HasValue);

                IEnumerable<Client>? clients = pageNumber.HasValue && pageSize.HasValue
                    ? await clientRepository.GetPagedAsync(pageNumber.Value, pageSize.Value)
                    : await clientRepository.GetAllAsync();

                logger.LogInformation("[SERVICE] Successfully retrieved clients from repository. Client count: {ClientCount}",
                    clients?.Count() ?? 0);

                if (clients is null || !clients.Any())
                {
                    logger.LogWarning("[SERVICE] No clients found in the database. PageNumber: {PageNumber}, PageSize: {PageSize}",
                        pageNumber, pageSize);
                    return new ClientListDTO()
                    {
                        Count = 0,
                        Data = Enumerable.Empty<ClientDTO>()
                    };
                }

                logger.LogInformation("[SERVICE] Mapping {ClientCount} clients to DTOs", clients.Count());

                var clientDTOs = clients.Select(client => new ClientDTO()
                {
                    Id = client.User?.Id,
                    Name = client.Name,
                    Email = client.User?.Email,
                    EmailConfirmed = client.User?.EmailConfirmed ?? false,
                    PhoneNumber = client.User?.PhoneNumber,
                    CreatedAt = client.User?.CreatedAt
                }).ToList();

                logger.LogInformation("[SERVICE] Successfully mapped {ClientCount} clients to DTOs. Returning results",
                    clientDTOs.Count);

                return new ClientListDTO()
                {
                    Count = clientDTOs.Count,
                    Data = clientDTOs
                };
            }
            catch
            {
                logger.LogError("[SERVICE] Error occurred while retrieving clients. PageNumber: {PageNumber}, PageSize: {PageSize}",
                    pageNumber, pageSize);
                throw new TechnicalException();
            }
        }
    }
}