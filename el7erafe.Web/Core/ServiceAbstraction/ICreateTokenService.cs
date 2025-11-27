using DomainLayer.Models.IdentityModule;
using DomainLayer.Models.IdentityModule.Enums;

namespace ServiceAbstraction
{
    public interface ICreateTokenService
    {
        Task<string> CreateAndStoreTokenAsync(ApplicationUser user, TokenType tokenType);
    }
}
