using Microsoft.Extensions.DependencyInjection;
using Service.Email;
using Service.Helpers;
using ServiceAbstraction;

namespace Service
{
    public static class ServiceLayerServicesRegistiration
    {
        public static IServiceCollection AddServiceLayerServices(this IServiceCollection services)
        {
            services.AddScoped<ITechnicianFileService, TechnicianFileService>();
            services.AddScoped<ILookupService, LookupService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IOtpService, OtpService>();
            services.AddScoped<IClientAuthenticationService, ClientAuthenticationService>();
            services.AddScoped<ITechAuthenticationService, TechAuthenticationService>();
            services.AddScoped<ILoginService, LoginService>();
            services.AddScoped<OtpHelper>();
            services.AddScoped<ILogoutService, LogoutService>();
            services.AddScoped<ITokenBlocklistService, TokenBlocklistService>();
            return services;
        }
    }
}
