using DomainLayer.Contracts;
using DomainLayer.Exceptions;
using DomainLayer.Models.IdentityModule;
using DomainLayer.Models.IdentityModule.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ServiceAbstraction;
using Shared.DataTransferObject.ClientIdentityDTOs;

namespace Service
{
    public class ClientAuthenticationService(UserManager<ApplicationUser> userManager,
        IClientRepository clientRepository
        ,ILogger<ClientAuthenticationService> logger) : IClientAuthenticationService
    {
        public async Task<ClientDTO> RegisterClientAsync(ClientRegisterDTO clientRegisterDTO)
        {
            logger.LogInformation("[SERVICE] Checking phone number uniqueness: {Phone}", clientRegisterDTO.PhoneNumber);
            var phoneNumberFound = await clientRepository.ExistsAsync(clientRegisterDTO.PhoneNumber);

            if(phoneNumberFound)
            {
                logger.LogWarning("[SERVICE] Duplicate phone number detected: {Phone}", clientRegisterDTO.PhoneNumber);
                throw new PhoneNumberAlreadyExists(clientRegisterDTO.PhoneNumber);
            }

            logger.LogInformation("[SERVICE] Checking email uniqueness: {Email}", clientRegisterDTO.Email);
            var userFound = await clientRepository.EmailExistsAsync(clientRegisterDTO.Email);

            if(userFound)
            {
                logger.LogWarning("[SERVICE] Duplicate email detected: {Email}", clientRegisterDTO.Email);
                throw new EmailAlreadyExists(clientRegisterDTO.Email);
            }

            var user = new ApplicationUser
            {
                UserName = clientRegisterDTO.PhoneNumber,
                Email = clientRegisterDTO.Email,
                PhoneNumber = clientRegisterDTO.PhoneNumber,
                UserType = UserTypeEnum.Client
            };

            var result = await userManager.CreateAsync(user, clientRegisterDTO.Password);

            if(!result.Succeeded)
            {
                logger.LogError("[SERVICE] User creation failed for email: {Email}", clientRegisterDTO.Email);
                var errors = result.Errors.Select(e => e.Description).ToList();
                throw new BadRequestException(errors);
            }

            var client = new Client
            {
                Name = clientRegisterDTO.Name,
                UserId = user.Id
            };

            await clientRepository.CreateAsync(client);

            await userManager.AddToRoleAsync(user, "Client");

            logger.LogInformation("[SERVICE] Client registration completed for: {Email}", clientRegisterDTO.Email);

            return new ClientDTO
            {
                Name = client.Name,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                Token = "token-ToDo",
                RefreshToken = "refreshToken-ToDo"
            };
        }
    }
}
