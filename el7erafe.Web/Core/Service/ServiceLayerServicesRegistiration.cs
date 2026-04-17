using Microsoft.Extensions.DependencyInjection;
using Service.Chat;
using Service.Email;
using Service.Helpers;
using Service.Moderation;
using ServiceAbstraction;
using ServiceAbstraction.Chat;
using ServiceAbstraction.Moderation;

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
            services.AddScoped<IAdminLoginService, AdminLoginService>();
            services.AddScoped<IAdminDashboardService, AdminDashboardService>();
            services.AddScoped<IClientService, ClientService>();
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<ITechnicianFlowService, TechnicianFlowService>();
            services.AddScoped<IClientRealTimeService, ClientRealTimeService>();
            services.AddScoped<ITechnicianAvailabilityService, TechnicianAvailabilityService>();
            services.AddScoped<ITechnicianRealTimeService, TechnicianRealTimeService>();
            services.AddScoped<IClientTechnicianCommonService, ClientTechnicianCommonService>();
            services.AddScoped<IOfferService, OfferService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddHttpClient<IModerationService, ModerationService>();
            services.AddSignalR();
            return services;
        }
    }
}
