using DomainLayer.Contracts;
using DomainLayer.Exceptions;
using DomainLayer.Models.IdentityModule;
using DomainLayer.Models.IdentityModule.Enums;
using Microsoft.Extensions.Logging;
using ServiceAbstraction;
using Shared.DataTransferObject.AdminDTOs.Dashboard;
using Shared.DataTransferObject.LookupDTOs;

namespace Service
{
    public class AdminDashboardService(IClientRepository clientRepository, ITechnicianServicesRepository technicianServicesRepository,
        ITechnicianFileService fileService,
        IBlobStorageRepository blobStorageRepository,
        IBlockedUserRepository blockedUserRepository,
        ITechnicianRepository technicianRepository,
        IRejectionCommentsRepository rejectionCommentsRepository,
        ILogger<AdminDashboardService> logger,
        IRejectionRepository rejectionRepository,
        IUserTokenRepository userTokenRepository) : IAdminDashboardService
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

        public async Task<TechnicianListDTO> GetTechniciansAsync(int? pageNumber, int? pageSize)
        {
            pageNumber = (pageNumber.HasValue && pageNumber.Value < 1) ? 1 : pageNumber;
            pageSize = (pageSize.HasValue && pageSize.Value < 1) ? 10 : pageSize;
            logger.LogInformation("[SERVICE] GetTechincianss method started. PageNumber: {PageNumber}, PageSize: {PageSize}",
                pageNumber, pageSize);
            try
            {
                logger.LogInformation("[SERVICE] Retrieving Technicians from repository. Using pagination: {UsePagination}",
                    pageNumber.HasValue && pageSize.HasValue);

                IEnumerable<Technician>? technicians = pageNumber.HasValue
                    ? await technicianRepository.GetPagedAsync(pageNumber.Value, pageSize.Value)
                    : await technicianRepository.GetAllAsync();

                logger.LogInformation("[SERVICE] Successfully retrieved Technicians from repository. Technician count: {TechnicianCount}",
                    technicians?.Count() ?? 0);

                if (technicians is null || !technicians.Any())
                {
                    logger.LogWarning("[SERVICE] No Technicians found in the database. PageNumber: {PageNumber}, PageSize: {pageSize}",
                        pageNumber, pageSize);
                    return new TechnicianListDTO()
                    {
                        Count = 0,
                        Data = Enumerable.Empty<TechnicianDTO>()
                    };
                }

                logger.LogInformation("[SERVICE] Mapping {TechnicianCount} Technicians to DTOs", technicians.Count());
                var technicianDTOs = technicians.Select(technician => new TechnicianDTO()
                {
                    id = technician.UserId,
                    name = technician.Name,
                    phone = technician.User?.PhoneNumber,
                    governorate = technician.City.Governorate.NameAr,
                    city = technician.City.NameAr,
                    faceIdImage = technician.NationalIdFrontURL,
                    backIdImage = technician.NationalIdBackURL,
                    criminalRecordImage = technician.CriminalHistoryURL,
                    serviceType = technician.Service.NameAr
                }).ToList();

                logger.LogInformation("[SERVICE] Successfully mapped {TechnicianCount} Technicians to DTOs. Returning results",
                    technicianDTOs.Count);

                return new TechnicianListDTO()
                {
                    Count = technicianDTOs.Count,
                    Data = technicianDTOs
                };
            }
            catch (Exception)
            {
                logger.LogError("[SERVICE] Error occurred while retrieving technicians. PageNumber: {PageNumber}, PageSize: {PageSize}",
                        pageNumber, pageSize);
                throw new TechnicalException();
            }
        }

        public async Task DeleteTechnicianAsync(string userId)
        {
            logger.LogInformation("[SERVICE] DeleteTechnicianAsync called for user ID: {UserId}", userId);
            logger.LogInformation("[SERVICE] Attempting to delete technician from repository for user ID: {UserId}", userId);
            var technicianDeleted = await technicianRepository.DeleteAsync(userId);
            if (technicianDeleted == 0)
            {
                logger.LogWarning("[SERVICE] Technician deletion failed - User not found: {UserId}", userId);
                throw new UserNotFoundException("المستخدم غير موجود");
            }
            logger.LogInformation("[SERVICE] Technician successfully deleted for user ID: {UserId}", userId);
        }

        public async Task<RejectionCommentsResponseDTO> GetRejectionCommentsAsync()
        {
            try
            {
                logger.LogInformation("[SERVICE] Retrieving all rejection comments");
                var rejectionComments = await rejectionCommentsRepository.GetAllRejectionCommentsAsync();
                if (rejectionComments == null || !rejectionComments.Any())
                {
                    logger.LogWarning("[SERVICE] No rejection comments found");
                    return new RejectionCommentsResponseDTO { data = new List<string>() };
                }

                var serviceNames = rejectionComments.Select(s => s.Reason).ToList();
                return new RejectionCommentsResponseDTO { data = serviceNames };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[SERVICE] Error retrieving all rejection comments");
                throw;
            }
        }

        public async Task<TechnicianListDTO> GetTechnicianRequestsAsync(int? pageNumber, int? pageSize, TechnicianStatus technicianStatus)
        {
            pageNumber = (pageNumber.HasValue && pageNumber.Value < 1) ? 1 : pageNumber;
            pageSize = (pageSize.HasValue && pageSize.Value < 1) ? 10 : pageSize;
            logger.LogInformation("[SERVICE] GetTechnicianRequests method started. PageNumber: {PageNumber}, PageSize: {PageSize}",
                pageNumber, pageSize);

            try
            {
                logger.LogInformation("[SERVICE] Retrieving {TechncianStatus} technicians from repository. Using pagination: {UsePagination}",
                    technicianStatus, pageNumber.HasValue && pageSize.HasValue);

                IEnumerable<Technician>? technicians = pageNumber.HasValue && pageSize.HasValue
                    ? await technicianRepository.GetPagedByStatusAsync(technicianStatus, pageNumber.Value, pageSize.Value)
                    : await technicianRepository.GetAllByStatusAsync(technicianStatus);

                logger.LogInformation("[SERVICE] Successfully retrieved technicians from repository. Technician count: {TechnicianCount}", technicians?.Count() ?? 0);
                var technicianDTOs = new List<TechnicianDTO>();
                foreach (var technician in technicians)
                {
                    technicianDTOs.Add(new TechnicianDTO()
                    {
                        id = technician.UserId,
                        name = technician.Name,
                        phone = technician.User?.PhoneNumber,
                        governorate = technician.City.Governorate.NameAr,
                        city = technician.City.NameAr,
                        faceIdImage = technician.NationalIdFrontURL,
                        backIdImage = technician.NationalIdBackURL,
                        criminalRecordImage = technician.CriminalHistoryURL,
                        serviceType = technician.Service.NameAr,
                        approvalStatus = technician.Status.ToString(),
                        is_Blocked = await blockedUserRepository.IsBlockedAsync(technician.UserId)
                    });
                }
                logger.LogInformation("[SERVICE] Successfully mapped {TechnicianCount} Technicians to DTOs. Returning results",
                    technicianDTOs.Count);
                return new TechnicianListDTO()
                {
                    Count = technicianDTOs.Count,
                    Data = technicianDTOs
                };
            }
            catch (Exception)
            {

                logger.LogError("[SERVICE] Error occurred while retrieving technicians. PageNumber: {PageNumber}, PageSize: {PageSize}, TechnicianStatus: {TechnicianStatus}",
                        pageNumber, pageSize, technicianStatus);
                throw new TechnicalException();
            }
        }

        public async Task ApproveTechnicianAsync(string userId)
        {
            logger.LogInformation("[SERVICE] Search for technician with user ID: {UserId}", userId);
            var technician = await technicianRepository.GetByUserIdAsync(userId);
            if (technician is null)
            {
                logger.LogWarning("[SERVICE] Technician approval failed - User not found: {UserId}", userId);
                throw new UserNotFoundException("المستخدم غير موجود");
            }

            if (technician.Status == TechnicianStatus.Accepted)
            {
                logger.LogWarning("[SERVICE] Technician approval failed - Already approved: {UserId}", userId);
                throw new BadRequestException(new List<string> { "الفني معتمد بالفعل" });
            }
            else if (technician.Status == TechnicianStatus.Blocked)
            {
                logger.LogWarning("[SERVICE] Technician approval failed - Currently blocked: {UserId}", userId);
                throw new BadRequestException(new List<string> { "الفني محظور ولا يمكن الموافقة عليه" });
            }
            else if (technician.Status == TechnicianStatus.Rejected)
            {
                logger.LogWarning("[SERVICE] Technician approval failed - Previously rejected: {UserId}", userId);
                throw new BadRequestException(new List<string> { "الفني مرفوض ولا يمكن الموافقة عليه الا بعد تحديث البيانات الخاصه به" });
            }
            else
            {
                technician.Status = TechnicianStatus.Accepted;
                await technicianRepository.UpdateAsync(technician);
                logger.LogInformation("[SERVICE] Technician with user ID: {UserId} has been approved successfully", userId);
            }
        }
        public async Task RejectTechnicianAsync(RejectTechDTO rejectTechDTO)
        {
            logger.LogInformation("[SERVICE] Search for technician with user ID: {UserId}", rejectTechDTO.id);
            var technician = await technicianRepository.GetByUserIdAsync(rejectTechDTO.id);
            if (technician is null)
            {
                logger.LogWarning("[SERVICE] Technician rejection failed - User not found: {UserId}", rejectTechDTO.id);
                throw new UserNotFoundException("المستخدم غير موجود");
            }

            if (technician.Status == TechnicianStatus.Rejected)
            {
                logger.LogWarning("[SERVICE] Technician rejection failed - Already rejected: {UserId}", rejectTechDTO.id);
                throw new BadRequestException(new List<string> { "الفني مرفوض بالفعل" });
            }
            else if (technician.Status == TechnicianStatus.Blocked)
            {
                logger.LogWarning("[SERVICE] Technician rejection failed - Currently blocked: {UserId}", rejectTechDTO.id);
                throw new BadRequestException(new List<string> { "الفني محظور ولا يمكن رفضه" });
            }
            else if (technician.Status == TechnicianStatus.Accepted)
            {
                logger.LogWarning("[SERVICE] Technician rejection failed - Already accepted: {UserId}", rejectTechDTO.id);
                throw new BadRequestException(new List<string> { "الفني معتمد ولا يمكن رفضه" });
            }
            else
            {
                if (rejectTechDTO.is_front_rejected == false && rejectTechDTO.is_back_rejected == false && rejectTechDTO.is_criminal_rejected == false)
                {
                    throw new BadRequestException(new List<string> { "يجب رفض صورة واحدة على الأقل" });
                }
                technician.Rejection_Count += 1;
                if (technician.Rejection_Count >= 3)
                {
                    var blockedUser = new BlockedUser()
                    {
                        UserId = technician.UserId,
                        EndDate = null,
                        SuspensionReason = "تجاوز الحد الأقصى لمرات الرفض",
                    };
                    technician.Status = TechnicianStatus.Blocked;
                    logger.LogInformation("[SERVICE] Technician with user ID: {UserId} has been blocked due to exceeding rejection limit", rejectTechDTO.id);
                    await blockedUserRepository.AddAsync(blockedUser);
                    technician.IsNationalIdFrontRejected = rejectTechDTO.is_front_rejected;
                    technician.IsNationalIdBackRejected = rejectTechDTO.is_back_rejected;
                    technician.IsCriminalHistoryRejected = rejectTechDTO.is_criminal_rejected;
                    await technicianRepository.UpdateAsync(technician);
                    await userTokenRepository.DeleteUserTokenAsync(technician.UserId);
                    throw new BadRequestException(new List<string> { "تم حظر الفني لتجاوزه عدد مرات الرفض" });
                }
                else
                {
                    var rejection = new Rejection()
                    {
                        TechnicianId = technician.Id,
                        Reason = rejectTechDTO.rejectionReason
                    };
                    technician.Status = TechnicianStatus.Rejected;
                    logger.LogInformation("[SERVICE] Technician with user ID: {UserId} has been rejected", rejectTechDTO.id);
                    await rejectionRepository.CreateAsync(rejection);
                    technician.IsNationalIdFrontRejected = rejectTechDTO.is_front_rejected;
                    technician.IsNationalIdBackRejected = rejectTechDTO.is_back_rejected;
                    technician.IsCriminalHistoryRejected = rejectTechDTO.is_criminal_rejected;
                    await technicianRepository.UpdateAsync(technician);
                    await userTokenRepository.DeleteUserTokenAsync(technician.UserId);
                    logger.LogInformation("[SERVICE] Rejection reason recorded for technician with user ID: {UserId}", rejectTechDTO.id);
                }
            }
        }
    }
}


