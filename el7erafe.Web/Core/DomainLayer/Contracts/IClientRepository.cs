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
        Task<bool> UpdateAsync(Client client);
        Task<bool> DeleteAsync(string userId);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByUserIdAsync(string userId);
        Task<bool> ExistsAsync(string phoneNumber);
        Task<bool> EmailExistsAsync(string email);
    }
}
