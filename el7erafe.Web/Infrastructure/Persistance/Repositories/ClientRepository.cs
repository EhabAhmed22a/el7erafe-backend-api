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

        public async Task<int> UpdateAsync(Client client)
        {
            var existingClient = await context.Set<Client>()
            .FirstOrDefaultAsync(c => c.Id == client.Id);

            if (existingClient is null)
                return -1;

            context.Entry(existingClient).CurrentValues.SetValues(client);

            return await context.SaveChangesAsync();
        }

        public async Task<int> DeleteAsync(int id)
        {
            var existingClient = await context.Set<Client>()
            .FirstOrDefaultAsync(c => c.Id == id);

            if (existingClient is null)
                return -1;

            context.Entry(existingClient).CurrentValues.SetValues(existingClient);

            return await context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await context.Set<Client>().AnyAsync(c => c.Id == id);
        }

        public async Task<bool> ExistsAsync(string phoneNumber)
        {
            return await context.Set<ApplicationUser>().AnyAsync(c => c.UserName == phoneNumber);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await context.Set<Client>().AnyAsync(c => c.User.Email == email);
        }
    }
}
