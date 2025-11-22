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
        IConfiguration _configuration) : ITechAuthenticationService
    {
        public async Task<TechDTO> techRegisterAsync(TechRegisterDTO techRegisterDTO)
        {

            _logger.LogInformation("[SERVICE] Checking phone number uniqueness: {Phone}", techRegisterDTO.PhoneNumber);
            var phoneNumberFound = await _technicianRepository.ExistsAsync(techRegisterDTO.PhoneNumber);

            if(phoneNumberFound)
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

            TechRegisterToReturnDTO processedData ;

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

            var CreateToken = new CreateToken(_userManager, _configuration); // Assuming IConfiguration is not needed here
            // Return TechDTO (assuming TechDTO has Name and PhoneNumber)
            return new TechDTO
            {
                tempToken = await CreateToken.CreateTokenAsync(user,tempToken:true)
            };
        }

        // In TechAuthenticationService.cs
        public async Task<UserDTO> CheckTechnicianApprovalAsync(string userId)
        {
            _logger.LogInformation("[SERVICE] Checking technician approval status for user: {UserId}", userId);

            // Get user
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("[SERVICE] User not found: {UserId}", userId);
                throw new UserNotFoundException("المستخدم غير موجود");
            }

            // Check if user is a technician
            var isTechnician = await _userManager.IsInRoleAsync(user, "Technician");
            if (!isTechnician)
            {
                _logger.LogWarning("[SERVICE] User is not a technician: {UserId}", userId);
                throw new TechNotFoundException(userId);
            }

            // Get technician record
            var technician = await _technicianRepository.GetByUserIdAsync(userId);
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

                    // Generate new token
                    var createToken = new CreateToken(_userManager, _configuration);
                    var token = await createToken.CreateTokenAsync(user,tempToken:false);

                    return new UserDTO
                    {
                        token = token,
                        userId = technician.UserId,
                        userName = technician.Name,
                        type = 'T'
                    };

                case TechnicianStatus.Pending:
                    _logger.LogWarning("[SERVICE] Technician pending approval: {UserId}", userId);
                    throw new TechnicianPendingException();

                case TechnicianStatus.Rejected:
                    _logger.LogWarning("[SERVICE] Technician rejected: {UserId}", userId);
                    throw new TechnicianRejectedException();

                default:
                    _logger.LogWarning("[SERVICE] Unknown technician status: {Status} for user: {UserId}", technician.Status, userId);
                    throw new TechnicianPendingException();
            }
        }
    }
}
