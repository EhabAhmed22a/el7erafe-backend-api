using Azure.Identity;
using Azure.Storage.Blobs;
using DomainLayer.Contracts;
using DomainLayer.Models.IdentityModule;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistance.Databases;
using Persistance.Repositories;
using Microsoft.Extensions.Hosting;

namespace Persistance
{
    public static class PersistanceServicesRegistration
    {
        public static IServiceCollection AddPersistanceServices(this IServiceCollection services,
            IConfiguration configuration, IWebHostEnvironment env)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                );

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 10;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();


            services.AddMemoryCache();
            services.AddScoped<IDataSeeding, DataSeeding>();
            services.AddScoped<ITechnicianRepository, TechnicianRepository>();
            services.AddScoped<IClientRepository, ClientRepository>();
            services.AddScoped<IBlobStorageRepository, BlobStorageRepository>();
            services.AddScoped<IUserTokenRepository, UserTokenRepository>();
            services.AddScoped<IAdminRepository, AdminRepository>();
            services.AddScoped<ITechnicianServicesRepository, TechnicianServicesRepository>();
            services.AddScoped<IBlockedUserRepository, BlockedUserRepository>();
            services.AddScoped<IRejectionCommentsRepository, RejectionCommentsRepository>();
            services.AddScoped<IRejectionRepository, RejectionRepository>();
            services.AddScoped<IServiceRequestRepository, ServiceRequestRepository>();
            services.AddScoped<ICityRepository, CityRepository>();
            services.AddScoped<IUserDelegationKeyCache, UserDelegationKeyCache>();
            services.AddSingleton<BlobServiceClient>(serviceProvider =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var env = serviceProvider.GetRequiredService<IWebHostEnvironment>();

                if (env.IsDevelopment())
                {
                    var connectionString = configuration.GetConnectionString("AzureBlobStorage");
                    return new BlobServiceClient(connectionString);
                }
                else
                {
                    var accountName = configuration.GetValue<string>("AzureBlobStorage:AccountName");
                    if (string.IsNullOrEmpty(accountName))
                    {
                        throw new InvalidOperationException("AzureBlobStorage AccountName is not configured for production environment.");
                    }
                    var blobServiceUri = new Uri($"https://{accountName}.blob.core.windows.net");
                    return new BlobServiceClient(blobServiceUri, new ManagedIdentityCredential());
                }
            });

            return services;
        }
    }
}

