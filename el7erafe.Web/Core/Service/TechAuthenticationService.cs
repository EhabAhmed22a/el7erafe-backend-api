using DomainLayer.Contracts;
using DomainLayer.Exceptions;
using DomainLayer.Models.IdentityModule;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServiceAbstraction;
using Shared.DataTransferObject.TechnicianIdentityDTOs;

namespace Service
{
    public class TechAuthenticationService(UserManager<ApplicationUser> _userManager,
        ITechnicianRepository _technicianRepository) : ITechAuthenticationService
    {
        public async Task<TechDTO> techLoginAsync(TechLoginDTO techLoginDTO)
        {
            // Find user by phone number
            var user = await _userManager.FindByIdAsync(techLoginDTO.PhoneNumber);

            if (user is null)
                throw new TechNotFoundException(techLoginDTO.PhoneNumber);

            // Check if user is a technician
            if (user.UserType != UserTypeEnum.Technician)
                throw new UnauthorizedTechException();

            // Check password
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, techLoginDTO.Password);
            if (!isPasswordValid)
                throw new UnauthorizedTechException();

            // Get technician profile
            var technician = await _technicianRepository.GetByUserIdAsync(user.Id);
            if (technician is null)
                throw new TechNotFoundException(techLoginDTO.PhoneNumber);

            // Return TechDTO
            return new TechDTO
            {
                Name = technician.Name,
                PhoneNumber = user.PhoneNumber,
                Status = technician.Status.ToString(),
                Token = "Token - TODO"
            };
        }

        public async Task<TechDTO> techRegisterAsync(TechRegisterDTO techRegisterDTO)
        {
            // Create User (TechRegisterDTO -> ApplicationUser)
            var user = new ApplicationUser()
            {
                UserName = techRegisterDTO.PhoneNumber, // Or use Email if available
                PhoneNumber = techRegisterDTO.PhoneNumber,
                UserType = UserTypeEnum.Technician,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, techRegisterDTO.Password);

            if (!result.Succeeded)
            {
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

            // Return TechDTO (assuming TechDTO has Name and PhoneNumber)
            return new TechDTO
            {
                Name = technician.Name,
                PhoneNumber = user.PhoneNumber,
                Status = technician.Status.ToString(),
                Token = "Token - TODO"
                // Add other properties as needed
            };
        }
    }
}
