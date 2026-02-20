
using DomainLayer.Models.IdentityModule;

namespace DomainLayer.Contracts
{
    public interface ICityRepository
    {
        Task<City?> GetCityNameById(int id);
        Task<Governorate?> GetGovernateByCityId(int id);
        Task<bool> ExistsAsync(int id);
        Task<City?> GetCityByNameAsync(string cityName);
    }
}
