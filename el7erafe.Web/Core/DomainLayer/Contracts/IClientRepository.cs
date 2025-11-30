using DomainLayer.Models.IdentityModule;

namespace DomainLayer.Contracts
{
    public interface IClientRepository
    {
        Task<Client> CreateAsync(Client client);
        Task<Client?> GetByIdAsync(int id);
        Task<Client?> GetByUserIdAsync(string userId);
        Task<IEnumerable<Client>?> GetAllAsync();
        Task<IEnumerable<Client>?> GetPagedAsync(int pageNumber, int pageSize);
        Task<int> UpdateAsync(Client client);
        Task<int> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsAsync(string phoneNumber);
        Task<bool> EmailExistsAsync(string email);
    }
}
