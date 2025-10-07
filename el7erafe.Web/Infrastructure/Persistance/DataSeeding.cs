using DomainLayer.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistance.Databases;
namespace Persistance
{
    public class DataSeeding(ApplicationDbContext _dbContext,
        RoleManager<IdentityRole> _roleManager) : IDataSeeding
    {
        public async Task IdentityDataSeedingAsync()
        {
            try
            {
                var pendingMigration = await _dbContext.Database.GetAppliedMigrationsAsync();
                if (pendingMigration.Any())
                {
                    await _dbContext.Database.MigrateAsync();
                }
                if (!_roleManager.Roles.Any())
                {
                    await _roleManager.CreateAsync(new IdentityRole("Client"));
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));
                    await _roleManager.CreateAsync(new IdentityRole("Technician"));
                }
                await _dbContext.SaveChangesAsync();

            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
