using DomainLayer.Contracts;
using DomainLayer.Models.IdentityModule;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistance.Databases;

namespace Persistance.Repositories
{
    public class ClientRepository(ApplicationDbContext context) : IClientRepository
    {
        public async Task<Client> CreateAsync(Client client)
        {
            await context.Set<Client>().AddAsync(client);
            await context.SaveChangesAsync();
            return client;
        }

        public async Task<Client?> GetByIdAsync(int id)
        {
            return await context.Set<Client>()
                        .Include(c => c.User)
                        .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Client?> GetByUserIdAsync(string userId)
        {
            return await context.Set<Client>()
                        .Include(c => c.User)
                        .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<IEnumerable<Client>?> GetAllAsync()
        {
            return await context.Set<Client>()
                .Include(c => c.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<Client>?> GetPagedAsync(int pageNumber, int pageSize)
        {
            return await context.Set<Client>()
                .Include(c => c.User)
                .OrderBy(c => c.User.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<bool> UpdateAsync(Client client)
        {
            var existingClient = await context.Set<Client>()
            .FirstOrDefaultAsync(c => c.Id == client.Id);

            if (existingClient is null)
                return false;

            context.Entry(existingClient).CurrentValues.SetValues(client);

            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(string userId)
        {
            int deletedRows = await context.Set<ApplicationUser>()
                                           .Where(u => u.Id == userId)
                                           .ExecuteDeleteAsync();

            // Returns true if the user existed and was deleted
            return deletedRows > 0;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await context.Set<Client>().AnyAsync(c => c.Id == id);
        }

        public async Task<bool> ExistsByUserIdAsync(string userId)
        {
            return await context.Set<Client>().AnyAsync(c => c.UserId == userId);
        }

        public async Task<bool> ExistsAsync(string phoneNumber)
        {
            return await context.Set<ApplicationUser>().AnyAsync(c => c.PhoneNumber == phoneNumber);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await context.Set<ApplicationUser>().AnyAsync(c => c.Email == email);
        }
    }
}
