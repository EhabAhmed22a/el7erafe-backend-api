
using DomainLayer.Contracts;
using DomainLayer.Models.IdentityModule;
using Microsoft.EntityFrameworkCore;
using Persistance.Databases;

namespace Persistance.Repositories
{
    public class CityRepository (ApplicationDbContext dbContext): ICityRepository
    {
        public async Task<City?> GetCityNameById(int id)
        {
            return await dbContext.Set<City>()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Governorate?> GetGovernateByCityId(int id)
        {
            var city = await dbContext.Set<City>().FirstOrDefaultAsync(c => c.Id == id);
            if(city is null)
                return null;
            return await dbContext.Set<Governorate>().FirstOrDefaultAsync(g => g.Id == city.GovernorateId);
        }
    }
}
