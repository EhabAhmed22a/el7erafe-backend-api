using Microsoft.Extensions.DependencyInjection;
using ServiceAbstraction;

namespace Service
{
    public static class ServiceLayerServicesRegistiration
    {
        public static IServiceCollection AddServiceLayerServices(this IServiceCollection services)
        {
            services.AddScoped<ITechnicianFileService, TechnicianFileService>();
            services.AddScoped<ILookupService, LookupService>();
            return services;
        }
    }
}
