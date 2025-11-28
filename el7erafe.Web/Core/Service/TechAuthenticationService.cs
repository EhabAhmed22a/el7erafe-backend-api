using DomainLayer.Contracts;
using DomainLayer.Exceptions;
using DomainLayer.Models.IdentityModule;
using DomainLayer.Models.IdentityModule.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ServiceAbstraction;
using Shared.DataTransferObject.LoginDTOs;
using Shared.DataTransferObject.TechnicianIdentityDTOs;

namespace Service
{
    public class TechAuthenticationService(UserManager<ApplicationUser> _userManager,
        ITechnicianRepository _technicianRepository,
        ITechnicianFileService _fileService,
        ILogger<TechAuthenticationService> _logger,
        IConfiguration _configuration,
        IUserTokenRepository _userTokenRepository,
        IBlobStorageRepository _blobStorageRepository) : ITechAuthenticationService
    {
        public async Task<TechDTO> techRegisterAsync(TechRegisterDTO techRegisterDTO)
        {

            _logger.LogInformation("[SERVICE] Checking phone number uniqueness: {Phone}", techRegisterDTO.PhoneNumber);
            var phoneNumberFound = await _technicianRepository.ExistsAsync(techRegisterDTO.PhoneNumber);

            if (phoneNumberFound)
            {
                _logger.LogWarning("[SERVICE] Duplicate phone number detected: {Phone}", techRegisterDTO.PhoneNumber);
                throw new PhoneNumberAlreadyExists(techRegisterDTO.PhoneNumber);
            }



            var governorate = await _technicianRepository.GetGovernorateByNameAsync(techRegisterDTO.Governorate);
            if (governorate == null)
            {
                _logger.LogWarning("[SERVICE] Governorate not found: {Governorate}", techRegisterDTO.Governorate);
                throw new GovernorateNotFoundException(techRegisterDTO.Governorate);
            }


            var city = await _technicianRepository.GetCityByNameAsync(techRegisterDTO.City, governorate.Id);
            if (city == null)
            {
                _logger.LogWarning("[SERVICE] City not found: {City} in Governorate: {Governorate}", techRegisterDTO.City, techRegisterDTO.Governorate);
                throw new CityNotFoundException(techRegisterDTO.City);
            }

            // **VALIDATE AND GET SERVICE DATA**
            _logger.LogInformation("[SERVICE] Validating service for: {Phone}", techRegisterDTO.PhoneNumber);

            var service = await _technicianRepository.GetServiceByNameAsync(techRegisterDTO.ServiceType);
            if (service == null)
            {
                _logger.LogWarning("[SERVICE] Service not found: {ServiceType}", techRegisterDTO.ServiceType);
                throw new ServiceNotFoundException(techRegisterDTO.ServiceType);
            }


            // Create User (TechRegisterDTO -> ApplicationUser)
            var user = new ApplicationUser()
            {
                UserName = techRegisterDTO.PhoneNumber,
                PhoneNumber = techRegisterDTO.PhoneNumber,
                UserType = UserTypeEnum.Technician,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, techRegisterDTO.Password);

            if (!result.Succeeded)
            {
                _logger.LogError("[SERVICE] User creation failed for PhoneNumber: {PhoneNumber}", techRegisterDTO.PhoneNumber);
                var errors = result.Errors.Select(e => e.Description).ToList();
                throw new BadRequestException(errors);
            }
            _logger.LogInformation("[SERVICE] Uploading technician documents to secure cloud storage for: {PhoneNumber}", techRegisterDTO.PhoneNumber);

            TechRegisterToReturnDTO processedData;

            try
            {
                processedData = await _fileService.ProcessTechnicianFilesAsync(techRegisterDTO);
            }
            catch (Exception)
            {
                await _userManager.DeleteAsync(user);
                throw new UnauthorizedBlobStorage();
            }

            _logger.LogInformation("[SERVICE] All technician documents securely stored in cloud storage successfully for: {PhoneNumber}", techRegisterDTO.PhoneNumber);

            // Create Technician
            var technician = new Technician
            {
                Name = processedData.Name,
                NationalIdFrontURL = processedData.NationalIdFrontPath,
                NationalIdBackURL = processedData.NationalIdBackPath,
                CriminalHistoryURL = processedData.CriminalRecordPath,
                UserId = user.Id,
                Status = TechnicianStatus.Pending,
                CityId = city.Id,
                ServiceId = service.Id
            };

            await _technicianRepository.CreateAsync(technician);

            await _userManager.AddToRoleAsync(user, "Technician");

            _logger.LogInformation("[SERVICE] Technician registration completed for: {PhoneNumber}", techRegisterDTO.PhoneNumber);

            var CreateToken = new CreateToken(_userManager, _configuration);
            string token = await CreateToken.CreateTokenAsync(user);

            var TechToken = new UserToken
            {
                Token = token,
                Type = TokenType.TempToken,
                UserId = user.Id
            };

            await _userTokenRepository.CreateUserTokenAsync(TechToken);

            return new TechDTO
            {
                tempToken = token
            };
        }

        public async Task<UserDTO> CheckTechnicianApprovalAsync(string userId)
        {
            _logger.LogInformation("[SERVICE] Checking technician approval status for user: {UserId}", userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("[SERVICE] User not found: {UserId}", userId);
                throw new UserNotFoundException("المستخدم غير موجود");
            }

            var technician = await _technicianRepository.GetFullTechnicianByUserIdAsync(userId);
            if (technician == null)
            {
                _logger.LogWarning("[SERVICE] Technician record not found for user: {UserId}", userId);
                throw new TechNotFoundException(userId);
            }

            _logger.LogInformation("[SERVICE] Technician status: {Status} for user: {UserId}", technician.Status, userId);

            // Handle different statuses
            switch (technician.Status)
            {
                case TechnicianStatus.Accepted:
                    _logger.LogInformation("[SERVICE] Technician approved: {UserId}", userId);

                    var usertoken = await _userTokenRepository.GetUserTokenAsync(userId);

                    if (usertoken.Type == TokenType.Token)
                    {
                        _logger.LogInformation("[SERVICE] Existing regular token found for user: {UserId}", userId);
                        return new UserDTO
                        {
                            token = usertoken.Token,
                            userId = technician.UserId,
                            userName = technician.Name,
                            type = 'T'
                        };
                    }
                    else
                    {
                        await _userTokenRepository.DeleteUserTokenAsync(userId);

                        var createToken = new CreateToken(_userManager, _configuration);
                        var accessToken = await createToken.CreateTokenAsync(user);

                        await _userTokenRepository.CreateUserTokenAsync(new UserToken
                        {
                            Token = accessToken,
                            Type = TokenType.Token,
                            UserId = user.Id
                        });

                        return new UserDTO
                        {
                            token = accessToken,
                            userId = technician.UserId,
                            userName = technician.Name,
                            type = 'T'
                        };
                    }

                case TechnicianStatus.Pending:
                    _logger.LogWarning("[SERVICE] Technician pending approval: {UserId}", userId);
                    var userToken = await _userTokenRepository.GetUserTokenAsync(userId);
                    throw new PendingTechnicianRequest(userToken.Token);

                case TechnicianStatus.Rejected:
                    _logger.LogWarning("[SERVICE] Technician rejected: {UserId}", userId);
                    await _userTokenRepository.DeleteUserTokenAsync(userId);
                    throw new RejectedTechnician(technician);

                case TechnicianStatus.Blocked:
                    _logger.LogWarning("[SERVICE] Technician is blocked: {UserId}", userId);
                    await _userTokenRepository.DeleteUserTokenAsync(userId);
                    throw new BlockedTechnician();

                default:
                    _logger.LogWarning("[SERVICE] Unknown technician status: {Status} for user: {UserId}", technician.Status, userId);
                    throw new BlockedTechnician();
            }
        }

        async Task<TechResubmitResponseDTO> ITechAuthenticationService.TechnicianResubmitDocumentsAsync(TechResubmitDTO techResubmitDTO)
        {
            _logger.LogInformation("[SERVICE] Checking User Existance: {Phone}", techResubmitDTO.PhoneNumber);

            var user = await _userManager.FindByNameAsync(techResubmitDTO.PhoneNumber);
            if (user is null)
            {
                _logger.LogInformation("[SERVICE] User with Phone Number: {Phone} is not exist", techResubmitDTO.PhoneNumber);
                throw new UserNotFoundException("المستخدم غير موجود");
            }
            var technician = await _technicianRepository.GetByUserIdAsync(user.Id);
            if (technician is null)
            {
                _logger.LogInformation("[SERVICE] Technician with UserId: {UserId} is not exist", user.Id);
                throw new TechNotFoundException(user.Id);
            }

            if (technician.Status == TechnicianStatus.Blocked)
            {
                _logger.LogInformation("[SERVICE] Technician with UserId: {UserId} is blocked", user.Id);
                throw new BlockedTechnician();
            }
            else if (technician.Status == TechnicianStatus.Accepted || technician.Status == TechnicianStatus.Pending)
            {
                _logger.LogInformation("[SERVICE] Technician with UserId: {UserId} is Already accepted", user.Id);
                throw new TechnicianAcceptedOrPendingException();
            }
            else
            {
                _logger.LogInformation("[SERVICE] Uploading technician re-submitted documents to secure cloud storage for: {PhoneNumber}", techResubmitDTO.PhoneNumber);

                TechReUploadFilesUrlDTO processedData;
                try
                {
                    processedData = await _fileService.ProcessTechnicianFileReUpload(techResubmitDTO);
                }
                catch (Exception ex)
                {
                    throw;
                }
                _logger.LogInformation("[SERVICE] All technician re-submitted documents securely stored in cloud storage successfully for: {PhoneNumber}", techResubmitDTO.PhoneNumber);
                var oldNationalIdFrontUrl = technician.NationalIdFrontURL;
                var oldNationalIdBackUrl = technician.NationalIdBackURL;
                var oldCriminalRecordUrl = technician.CriminalHistoryURL;

                technician.NationalIdFrontURL = processedData.NationalIdFrontUrl ?? technician.NationalIdFrontURL;
                technician.NationalIdBackURL = processedData.NationalIdBackUrl ?? technician.NationalIdBackURL;
                technician.CriminalHistoryURL = processedData.CriminalRecordUrl ?? technician.CriminalHistoryURL;
                technician.Status = TechnicianStatus.Pending;

                if (technician.NationalIdFrontURL != oldNationalIdFrontUrl)
                {
                    technician.IsNationalIdFrontVerified = false;
                    await _blobStorageRepository.DeleteFileAsync(oldNationalIdFrontUrl, "technician-documents");
                }
                if (technician.NationalIdBackURL != oldNationalIdBackUrl)
                {
                    technician.IsNationalIdBackVerified = false;
                    await _blobStorageRepository.DeleteFileAsync(oldNationalIdBackUrl, "technician-documents");
                }
                if (technician.CriminalHistoryURL != oldCriminalRecordUrl)
                {
                    technician.IsCriminalHistoryVerified = false;
                    await _blobStorageRepository.DeleteFileAsync(oldCriminalRecordUrl, "technician-documents");
                }
                await _technicianRepository.UpdateAsync(technician);
                _logger.LogInformation("[SERVICE] Technician re-submission completed for: {PhoneNumber}", techResubmitDTO.PhoneNumber);
                return new TechResubmitResponseDTO
                {
                    message = "تم إعادة إرسال المستندات بنجاح. يرجى انتظار المراجعة."
                };
            }
        }
    }
}
