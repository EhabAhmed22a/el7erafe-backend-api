using DomainLayer.Contracts;

namespace el7erafe.Web.Extensions
{
    public static class DatabaseSeed
    {
        public static async Task<WebApplication> SeedDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var dataSeedingObject = scope.ServiceProvider.GetRequiredService<IDataSeeding>();
            await dataSeedingObject.IdentityDataSeedingAsync();
            return app;
        }

    }
}
