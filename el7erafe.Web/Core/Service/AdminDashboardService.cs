using DomainLayer.Contracts;
using DomainLayer.Exceptions;
using DomainLayer.Models.IdentityModule;
using Microsoft.Extensions.Logging;
using ServiceAbstraction;
using Shared.DataTransferObject.AdminDTOs.Dashboard;

namespace Service
{
    public class AdminDashboardService(IClientRepository clientRepository, ITechnicianServicesRepository technicianServicesRepository,
        ITechnicianFileService fileService,
        ILogger<AdminDashboardService> logger) : IAdminDashboardService
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

        public async Task<ServiceListDTO> GetServicesAsync(int? pageNumber, int? pageSize)
        {
            pageNumber = (pageNumber.HasValue && pageNumber.Value < 1) ? 1 : pageNumber;
            pageSize = (pageSize.HasValue && pageSize.Value < 1) ? 10 : pageSize;

            logger.LogInformation("[SERVICE] GetServicesAsync method started. PageNumber: {PageNumber}, PageSize: {PageSize}",
                pageNumber, pageSize);

            try
            {
                logger.LogInformation("[SERVICE] Retrieving services from repository. Using pagination: {UsePagination}",
                    pageNumber.HasValue && pageSize.HasValue);

                IEnumerable<TechnicianService>? services = pageNumber.HasValue && pageSize.HasValue
                    ? await technicianServicesRepository.GetPagedTechnicianServicesAsync(pageNumber.Value, pageSize.Value)
                    : await technicianServicesRepository.GetAllTechnicianServicesAsync();

                logger.LogInformation("[SERVICE] Successfully retrieved services from repository. Services count: {ServicesCount}",
                    services?.Count() ?? 0);

                if (services is null || !services.Any())
                {
                    logger.LogWarning("[SERVICE] No services found in the database. PageNumber: {PageNumber}, PageSize: {PageSize}",
                        pageNumber, pageSize);
                    return new ServiceListDTO
                    {
                        Count = 0,
                        Services = Enumerable.Empty<ServiceDTO>()
                    };
                }

                logger.LogInformation("[SERVICE] Mapping {ServicesCount} services to DTOs", services.Count());

                var serviceDTOs = services.Select(ser => new ServiceDTO()
                {
                    Id = ser.Id,
                    Name = ser.NameAr,
                    ServiceImageURL = ser?.ServiceImageURL
                }).ToList();

                logger.LogInformation("[SERVICE] Successfully mapped {ServicesCount} services to DTOs. Returning results",
                    serviceDTOs.Count);

                return new ServiceListDTO
                {
                    Count = serviceDTOs.Count,
                    Services = serviceDTOs
                };
            }
            catch
            {
                logger.LogError("[SERVICE] Error occurred while retrieving services. PageNumber: {PageNumber}, PageSize: {PageSize}",
                    pageNumber, pageSize);
                throw new TechnicalException();
            }
        }

        public async Task<ServiceDTO> CreateServiceAsync(ServiceRegisterDTO serviceRegisterDTO)
        {
            logger.LogInformation("[SERVICE] CreateServiceAsync started for service: {ServiceName}",
                serviceRegisterDTO.Name);

            logger.LogInformation("[SERVICE] Checking if service already exists: {ServiceName}",
        serviceRegisterDTO.Name);

            if (await technicianServicesRepository.ServiceExistsAsync(serviceRegisterDTO.Name))
            {
                logger.LogWarning("[SERVICE] Service creation failed - already exists: {ServiceName}",
            serviceRegisterDTO.Name);
                throw new ServiceAlreadyRegisteredException();
            }

            logger.LogInformation("[SERVICE] Service does not exist, proceeding with creation: {ServiceName}",
       serviceRegisterDTO.Name);

            ServiceDTO serviceDTO;
            try
            {
                logger.LogInformation("[SERVICE] Processing service files for: {ServiceName}",
                    serviceRegisterDTO.Name);

                serviceDTO = await fileService.ProcessServiceFilesAsync(serviceRegisterDTO);

                logger.LogInformation("[SERVICE] Successfully processed files. Image URL: {ImageURL}",
                    serviceDTO.ServiceImageURL);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[SERVICE] Failed to process service files for: {ServiceName}",
                    serviceRegisterDTO.Name);
                throw new UnauthorizedBlobStorage();
            }

            try
            {
                logger.LogInformation("[SERVICE] Creating technician service record: {ServiceName}",
                    serviceDTO.Name);

                var createdService = await technicianServicesRepository.CreateServiceAsync(new TechnicianService()
                {
                    NameAr = serviceDTO.Name,
                    ServiceImageURL = serviceDTO.ServiceImageURL
                });

                logger.LogInformation("[SERVICE] Successfully created service with ID: {ServiceId}",
                    createdService.Id);

                serviceDTO.Id = createdService.Id;

                logger.LogInformation("[SERVICE] CreateServiceAsync completed successfully. Service ID: {ServiceId}, Name: {ServiceName}",
                    serviceDTO.Id, serviceDTO.Name);

                return serviceDTO;
            }
            catch
            {
                logger.LogError("[SERVICE] Failed to create service record in database for: {ServiceName}",
                    serviceDTO.Name);
                throw new TechnicalException();
            }
        }
    }
}