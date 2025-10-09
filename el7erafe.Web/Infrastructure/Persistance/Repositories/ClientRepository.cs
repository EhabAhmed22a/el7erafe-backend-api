using DomainLayer.Contracts;
using DomainLayer.Models.IdentityModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Persistance.Databases;

namespace Persistance.Repositories
{
    public class ClientRepository(ApplicationDbContext context, ILogger<ClientRepository> logger) : IClientRepository
    {
        public async Task<Client> CreateAsync(Client client)
        {
            await context.Set<Client>().AddAsync(client);
            await context.SaveChangesAsync();
            logger.LogInformation(
                "Client with Id {ClientId} created successfully with UserId {UserId} using CreateAsync",
                client.Id, client.UserId);
            return client;
        }

        public async Task<Client?> GetByIdAsync(int id)
        {
            var client = await context.Set<Client>()
                        .Include(c => c.User)
                        .FirstOrDefaultAsync(c => c.Id == id);

            if (client is not null)
            {
                logger.LogInformation(
                    "Client {ClientId} found using GetByIdAsync for requested id {RequestedId}",
                    client.Id, id);
                return client;
            }

            logger.LogInformation(
                "Client not found using GetByIdAsync for requested id {RequestedId}",
                id);
            return client;
        }

        public async Task<Client?> GetByUserId(string userId)
        {
            var client = await context.Set<Client>()
                        .Include(c => c.User)
                        .FirstOrDefaultAsync(c => c.UserId == userId);

            if(client is not null)
            {
                logger.LogInformation(
                    "Client {ClientUserId} found using GetByUserIdAsync for requested userId {RequestedUserId}",
                    client.UserId, userId);
                return client;
            }
            logger.LogInformation(
                "Client not found using GetByUserIdAsync for requested id {RequestedUserId}",
                userId);
            return client;
        }

        public async Task<IEnumerable<Client>?> GetAllAsync()
        {
            var clients = await context.Set<Client>()
                .Include(c => c.User)
                .ToListAsync();
                
            if(clients.Any())
            {
                logger.LogInformation("Client list returned with {ClientCount} clients using GetAllAsync",
            clients.Count);
                return clients;
            }
            logger.LogInformation("Empty Client list returned using GetAllAsync");
            return clients;
        }

        public async Task<bool> UpdateAsync(Client client)
        {
            try
            {
                context.Set<Client>().Update(client);
                await context.SaveChangesAsync();
                logger.LogInformation("Client {ClientId} updated using UpdateAsync", client.Id);
                return true;
            }

            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to update client {ClientId}", client.Id);
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
                    logger.LogInformation("Client {ClientId} deleted using DeleteAsync for requested id {RequestedId}",
                        client.Id, id);
                    return true;
                }
                logger.LogInformation("Client {ClientId} not found for deletion", id);
                return false;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to delete client {ClientId}", id);
                return false;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await context.Set<Client>().AnyAsync(c => c.Id == id);
        }

        public async Task<bool> ExistsAsync(string userId)
        {
            return await context.Set<Client>().AnyAsync(c => c.UserId == userId);
        }
    }
}
