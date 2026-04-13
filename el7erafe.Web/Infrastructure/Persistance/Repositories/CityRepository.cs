
using DomainLayer.Contracts;
using DomainLayer.Models.IdentityModule;
using Microsoft.EntityFrameworkCore;
using Persistance.Databases;

namespace Persistance.Repositories
{
    public class CityRepository (ApplicationDbContext dbContext): ICityRepository
    {
        public async Task<bool> ExistsAsync(int id)
        {
            return await dbContext.Set<City>().AnyAsync(c => c.Id == id);
        }

        public async Task<City?> GetCityNameById(int id)
        {
            return await dbContext.Set<City>()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Governorate?> GetGovernateByCityId(int id)
        {
            var city = await dbContext.Set<City>()
                .Include(c => c.Governorate)
                .FirstOrDefaultAsync(c => c.Id == id);

            return city?.Governorate;
        }

        public async Task<City?> GetCityByNameAsync(string cityName)
        {
            return await dbContext.Set<City>().Include(c => c.Governorate).FirstOrDefaultAsync(c => c.NameAr == cityName);
        }
    }
}
