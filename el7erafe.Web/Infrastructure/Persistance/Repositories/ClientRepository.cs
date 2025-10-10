using DomainLayer.Contracts;
using DomainLayer.Models.IdentityModule;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistance.Databases;

namespace Persistance.Repositories
{
    public class ClientRepository(ApplicationDbContext context, ILogger<ClientRepository> logger) : IClientRepository
    {
        public async Task<Client> CreateAsync(Client client)
        {
            await context.Set<Client>().AddAsync(client);
            await context.SaveChangesAsync();
            return client;
        }

        public async Task<Client?> GetByIdAsync(int id)
        {
            var client = await context.Set<Client>()
                        .Include(c => c.User)
                        .FirstOrDefaultAsync(c => c.Id == id);
            return client;
        }

        public async Task<Client?> GetByUserId(string userId)
        {
            var client = await context.Set<Client>()
                        .Include(c => c.User)
                        .FirstOrDefaultAsync(c => c.UserId == userId);
            return client;
        }

        public async Task<IEnumerable<Client>?> GetAllAsync()
        {
            var clients = await context.Set<Client>()
                .Include(c => c.User)
                .ToListAsync();
            return clients;
        }

        public async Task<bool> UpdateAsync(Client client)
        {
            try
            {
                context.Set<Client>().Update(client);
                await context.SaveChangesAsync();
                return true;
            }

            catch (SqlException ex)
            {
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var client = await context.Set<Client>().FindAsync(id);
                if (client is not null)
                {
                    context.Set<Client>().Remove(client);
                    await context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (SqlException ex)
            {
                return false;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await context.Set<Client>().AnyAsync(c => c.Id == id);
        }

        public async Task<bool> ExistsAsync(string phoneNumber)
        {
            return await context.Set<Client>().AnyAsync(c => c.User.PhoneNumber == phoneNumber);
        }
    }
}
