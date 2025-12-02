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
        IBlobStorageRepository blobStorageRepository,
        IBlockedUserRepository blockedUserRepository,
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
                    CreatedAt = client.User?.CreatedAt,
                    IsBlocked = blockedUserRepository.IsBlockedAsync(client.User?.Id!).Result
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
                    ? await technicianServicesRepository.GetPagedAsync(pageNumber.Value, pageSize.Value)
                    : await technicianServicesRepository.GetAllAsync();

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

            if (await technicianServicesRepository.ExistsAsync(serviceRegisterDTO.Name))
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

                var createdService = await technicianServicesRepository.CreateAsync(new TechnicianService()
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

        public async Task DeleteServiceAsync(int id)
        {
            logger.LogInformation("Attempting to delete service with ID: {id}", id);

            bool result = await technicianServicesRepository.DeleteAsync(id);

            if (result is false)
            {
                logger.LogWarning("Service with ID: {id} not found for deletion", id);
                throw new ServiceDoesNotExistException();
            }

            logger.LogInformation("Successfully deleted service with ID: {id}", id);
        }

        public async Task DeleteClientAsync(string userId)
        {
            logger.LogInformation("[SERVICE] DeleteClientAsync called for user ID: {UserId}", userId);

            logger.LogInformation("[SERVICE] Attempting to delete client from repository for user ID: {UserId}", userId);

            var clientDeleted = await clientRepository.DeleteAsync(userId);

            if (clientDeleted is false)
            {
                logger.LogWarning("[SERVICE] Client deletion failed - User not found: {UserId}", userId);
                throw new UserNotFoundException("المستخدم غير موجود");
            }

            logger.LogInformation("[SERVICE] Client successfully deleted for user ID: {UserId}", userId);
        }

        public async Task UpdateServiceAsync(int id, ServiceUpdateDTO serviceUpdateDTO)
        {
            logger.LogInformation("[SERVICE] UpdateServiceAsync called for Service ID: {ServiceId}", id);

            var isServiceAvailable = await technicianServicesRepository.ExistsAsync(id);
            if (isServiceAvailable is false)
            {
                logger.LogWarning("[SERVICE] Service update failed: Service not found with ID: {ServiceId}", id);
                throw new ServiceDoesNotExistException();
            }

            logger.LogInformation("[SERVICE] Service found with ID: {ServiceId}, retrieving details", id);
            var existingService = await technicianServicesRepository.GetByIdAsync(id);

            logger.LogInformation("[SERVICE] Creating update object for Service ID: {ServiceId}", id);
            var updatedService = new TechnicianService()
            {
                Id = id,
                NameAr = existingService!.NameAr,
                ServiceImageURL = existingService!.ServiceImageURL,
            };

            if (!string.IsNullOrEmpty(serviceUpdateDTO.service_name))
            {
                logger.LogInformation("[SERVICE] New service name provided: {ServiceName}", serviceUpdateDTO.service_name);

                if (await technicianServicesRepository.ExistsAsync(serviceUpdateDTO.service_name))
                {
                    logger.LogWarning("[SERVICE] Service update failed: Service name already exists: {ServiceName}", serviceUpdateDTO.service_name);
                    throw new ServiceAlreadyRegisteredException();
                }

                updatedService.NameAr = serviceUpdateDTO.service_name;
                logger.LogInformation("[SERVICE] Service name updated to: {ServiceName}", serviceUpdateDTO.service_name);
            }

            if (serviceUpdateDTO.service_image is not null)
            {
                logger.LogInformation("[SERVICE] New service image provided: {FileName} ({Size} bytes)",
                    serviceUpdateDTO.service_image.FileName, serviceUpdateDTO.service_image.Length);

                string imageURL = await blobStorageRepository.UploadFileAsync(serviceUpdateDTO.service_image,
                    "services-documents",
                    $"{serviceUpdateDTO.service_image.FileName}{Guid.NewGuid()}");

                logger.LogInformation("[SERVICE] Image uploaded successfully. New URL: {ImageUrl}", imageURL);
                updatedService.ServiceImageURL = imageURL;
            }

            logger.LogInformation("[SERVICE] Calling repository to update service with ID: {ServiceId}", id);
            bool updated = await technicianServicesRepository.UpdateAsync(updatedService);

            if (!updated)
            {
                logger.LogError("[SERVICE] Service update failed: Repository returned false for Service ID: {ServiceId}", id);
                throw new TechnicalException();
            }

            logger.LogInformation("[SERVICE] Service updated successfully for ID: {ServiceId}", id);
        }

        public async Task BlockUnblockClientAsync(BlockUnblockDTO blockDTO, string userId)
        {
            var existingUser = await clientRepository.GetByUserIdAsync(userId);
            if (existingUser is null)
            {
                throw new UserNotFoundException("المستخدم غير موجود");
            }

            if (blockDTO.IsBlocked is true && blockDTO.SuspendTo is not null)
            {
                if (await blockedUserRepository.IsBlockedAsync(userId))
                {
                    throw new BadRequestException(new List<string> { "المستخدم محظور مؤقتا بالفعل" });
                }

                if (await blockedUserRepository.IsPermBlockedAsync(userId))
                {
                    throw new BadRequestException(new List<string> { "المستخدم محظور دائما بالفعل" });
                }

                if (blockDTO.SuspendTo.Value.Date <= DateTime.UtcNow.Date)
                {
                    throw new BadRequestException(new List<string> { "تاريخ التعليق غير صحيح" });
                }

                var blockAudit = new BlockedUser()
                {
                    EndDate = blockDTO.SuspendTo,
                    SuspensionReason = blockDTO.SuspensionReason,
                    UserId = userId
                };
                await blockedUserRepository.AddAsync(blockAudit);
            }

            else if (blockDTO.IsBlocked is true && blockDTO.SuspendTo is null)
            {
                if (await blockedUserRepository.IsPermBlockedAsync(userId))
                {
                    throw new BadRequestException(new List<string> { "المستخدم محظور دائما بالفعل" });
                }

                var existingBlock = await blockedUserRepository.GetByUserIdAsync(userId);
                if (existingBlock != null)
                {
                    existingBlock.EndDate = null;
                    existingBlock.SuspensionReason = blockDTO.SuspensionReason;
                    await blockedUserRepository.UpdateAsync(existingBlock);
                }
                else
                {
                    var blockAudit = new BlockedUser()
                    {
                        EndDate = null,
                        SuspensionReason = blockDTO.SuspensionReason,
                        UserId = userId
                    };
                    await blockedUserRepository.AddAsync(blockAudit);
                }
            }

            else if (blockDTO.IsBlocked is false)
            {
                if (blockDTO.SuspendTo is not null || blockDTO.SuspensionReason is not null)
                {
                    throw new BadRequestException(new List<string> { "لا يمكن تحديد تاريخ التعليق أو السبب عندما يكون المستخدم غير محظور" });
                }

                if (!await blockedUserRepository.IsBlockedAsync(userId) && !await blockedUserRepository.IsPermBlockedAsync(userId))
                    throw new BadRequestException(new List<string> { "المستخدم غير محظور بالفعل" });

                await blockedUserRepository.RemoveAsync(userId);
            }
        }
    }
}


