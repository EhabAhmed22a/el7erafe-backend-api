using DomainLayer.Contracts;
using DomainLayer.Exceptions;
using DomainLayer.Models.IdentityModule;
using DomainLayer.Models.IdentityModule.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ServiceAbstraction;
using Shared.DataTransferObject.TechnicianIdentityDTOs;

namespace Service
{
    public class TechAuthenticationService(UserManager<ApplicationUser> _userManager,
        ITechnicianRepository _technicianRepository,
        ILogger<TechAuthenticationService> _logger) : ITechAuthenticationService
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

            _logger.LogInformation("[SERVICE] Checking National Id uniqueness: {NationalId}", techRegisterDTO.NationalId);
            var NationalIdFound = await _technicianRepository.ExistsByNationalIdAsync(techRegisterDTO.NationalId);

            if (NationalIdFound)
            {
                _logger.LogWarning("[SERVICE] Duplicate phone number detected: {Phone}", techRegisterDTO.PhoneNumber);
                throw new NationalIdAlreadyExists(techRegisterDTO.NationalId);
            }

            // Create User (TechRegisterDTO -> ApplicationUser)
            var user = new ApplicationUser()
            {
                UserName = techRegisterDTO.PhoneNumber, 
                PhoneNumber = techRegisterDTO.PhoneNumber,
                UserType = UserTypeEnum.Technician
            };

            var result = await _userManager.CreateAsync(user, techRegisterDTO.Password);

            if (!result.Succeeded)
            {
                _logger.LogError("[SERVICE] User creation failed for PhoneNumber: {PhoneNumber}", techRegisterDTO.PhoneNumber);
                var errors = result.Errors.Select(e => e.Description).ToList();
                throw new BadRequestException(errors);
            }

            // Create Technician
            var technician = new Technician
            {
                Name = techRegisterDTO.Name,
                NationalId = techRegisterDTO.NationalId,
                NationalIdFrontURL = techRegisterDTO.NationalIdFrontURL,
                NationalIdBackURL = techRegisterDTO.NationalIdBackURL,
                CriminalHistoryURL = techRegisterDTO.CriminalRecordURL,
                UserId = user.Id,
                Status = TechnicianStatus.Pending
            };

            await _technicianRepository.CreateAsync(technician);

            // Assign the Technician role
            await _userManager.AddToRoleAsync(user, "Technician");

            _logger.LogInformation("[SERVICE] Technician registration completed for: {PhoneNumber}", techRegisterDTO.PhoneNumber);

            // Return TechDTO (assuming TechDTO has Name and PhoneNumber)
            return new TechDTO
            {
                Name = technician.Name,
                PhoneNumber = user.PhoneNumber,
                Status = technician.Status.ToString(),
                Token = "Token - TODO"
            };
        }
    }
}
