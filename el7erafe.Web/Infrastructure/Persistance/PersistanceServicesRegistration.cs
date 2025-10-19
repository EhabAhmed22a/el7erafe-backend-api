using DomainLayer.Contracts;
using DomainLayer.Models.IdentityModule;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistance.Databases;
using Persistance.Repositories;
using Service;
using Service.Email;
using ServiceAbstraction;
using static DomainLayer.Contracts.IBlobStorage;

namespace Persistance
{
    public static class PersistanceServicesRegistration
    {
        public static IServiceCollection AddPersistanceServices(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                );

            services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 10;
            })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();


            services.AddMemoryCache();
            services.AddScoped<IDataSeeding, DataSeeding>();
            services.AddScoped<ITechnicianRepository, TechnicianRepository>();
            services.AddScoped<IClientRepository, ClientRepository>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IOtpService, OtpService>();
            services.AddScoped<IClientAuthenticationService, ClientAuthenticationService>();
            services.AddScoped<ITechAuthenticationService, TechAuthenticationService>();
            services.AddScoped<IBlobStorageService, BlobStorageService>();

            return services;
        }
    }
}

