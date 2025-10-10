using DomainLayer.Contracts;
using DomainLayer.Exceptions;
using DomainLayer.Models.IdentityModule;
using DomainLayer.Models.IdentityModule.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServiceAbstraction;
using Shared.DataTransferObject.TechnicianIdentityDTOs;

namespace Service
{
    public class TechAuthenticationService(UserManager<ApplicationUser> _userManager,
        ITechnicianRepository _technicianRepository) : ITechAuthenticationService
    {
        public async Task<TechDTO> techRegisterAsync(TechRegisterDTO techRegisterDTO)
        {
            // Create User (TechRegisterDTO -> ApplicationUser)
            var user = new ApplicationUser()
            {
                UserName = techRegisterDTO.PhoneNumber, // Or use Email if available
                PhoneNumber = techRegisterDTO.PhoneNumber,
                UserType = UserTypeEnum.Technician
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
