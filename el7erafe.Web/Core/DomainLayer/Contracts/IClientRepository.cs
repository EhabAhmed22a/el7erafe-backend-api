using DomainLayer.Models.IdentityModule;

namespace DomainLayer.Contracts
{
    public interface IClientRepository
    {
        Task<Client> CreateAsync(Client client);
        Task<Client?> GetByIdAsync(int id);
        Task<Client?> GetByUserId(string userId);
        Task<IEnumerable<Client>?> GetAllAsync();
        Task<bool> UpdateAsync(Client client);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsAsync(string phoneNumber);
    }
}
