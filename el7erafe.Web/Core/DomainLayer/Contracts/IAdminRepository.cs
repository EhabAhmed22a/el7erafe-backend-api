
using DomainLayer.Models.IdentityModule;

namespace DomainLayer.Contracts
{
    public interface IAdminRepository
    {
        Task<Admin?> GetByUserId(string userId);
    }
}
