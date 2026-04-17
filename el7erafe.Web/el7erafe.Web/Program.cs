using Azure.Identity;
using Azure.Storage.Blobs;
using el7erafe.Web.CustomMiddleWares;
using el7erafe.Web.Extensions;
using el7erafe.Web.Filters;
using Microsoft.AspNetCore.Authorization;
using Persistance;
using Presentation.Hubs;
using Serilog;
using Service;
using Service.Email;
using Service.Hubs;
using ServiceAbstraction;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

namespace el7erafe.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region Firebase Initialization

            var env = builder.Environment;

            if (!env.IsDevelopment())
            {
                var base64Credentials = builder.Configuration["FIREBASE_CREDENTIALS_BASE64"];

                if (!string.IsNullOrEmpty(base64Credentials) && FirebaseApp.DefaultInstance == null)
                {
                    try
                    {
                        byte[] decodedBytes = Convert.FromBase64String(base64Credentials);

                        using var stream = new MemoryStream(decodedBytes);

                        var credential = CredentialFactory.FromStream<ServiceAccountCredential>(stream);

                        FirebaseApp.Create(new AppOptions()
                        {
                            Credential = credential.ToGoogleCredential()
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Firebase init failed: {ex.Message}");
                    }
                }
            }
            #endregion

            #region Controller Settings
            builder.Services.AddControllers(options =>
            {
                options.Filters.Add<CustomValidationFilter>();
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressModelStateInvalidFilter = true; // Disable default
            });
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
        policy => policy
            .WithOrigins("http://localhost:4200",
                         "https://localhost:4200",
                         "https://7otob3den.com",
                         "http://127.0.0.1:5500",
                         "https://www.7otob3den.com")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
            });
            #endregion

            #region Json Options
            builder.Services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
            });
            #endregion

            #region SignalR
            builder.Services.AddSignalR(builder => builder.KeepAliveInterval = TimeSpan.FromSeconds(15)).AddAzureSignalR(options =>
            {
                options.ConnectionString = builder.Configuration["Azure:SignalR:ConnectionString"]; 
                options.InitialHubServerConnectionCount = 1;
            });
            #endregion

            #region Add services to the container.
            builder.Services.AddPersistanceServices(builder.Configuration, builder.Environment);
            builder.Services.AddJWTService(builder.Configuration);
            builder.Services.AddServiceLayerServices();
            #endregion

            #region Email Services
            builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("gmail"));
            builder.Services.AddScoped<IEmailService, EmailService>();
            #endregion

            #region Swagger Setup
            builder.Services.AddOpenApi();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                var presentationXmlPath = Path.Combine(AppContext.BaseDirectory, "Presentation.xml");
                if (File.Exists(presentationXmlPath))
                {
                    options.IncludeXmlComments(presentationXmlPath);
                }

                // Include Web XML
                var webXmlPath = Path.Combine(AppContext.BaseDirectory, "api.xml");
                if (File.Exists(webXmlPath))
                {
                    options.IncludeXmlComments(webXmlPath);
                }
            }
            );
            #endregion

            #region Serilog Setup
            builder.Host.UseSerilog((context, services,
                loggerConfiguration) =>
            {
                loggerConfiguration.ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "el7erafe");

                var accountName = context.Configuration["AzureBlobStorage:AccountName"];

                if (!string.IsNullOrEmpty(accountName))
                {
                    var blobServiceClient = new BlobServiceClient(
                        new Uri($"https://{accountName}.blob.core.windows.net"),
                        new DefaultAzureCredential());

                    loggerConfiguration.WriteTo.AzureBlobStorage(
                        blobServiceClient: blobServiceClient,
                        storageContainerName: "$logs",
                        storageFileName: "el7erafe-{yyyy-MM-dd}.log",
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}");
                }
            });
            #endregion

            #region Health Check MiddleWare
            builder.Services.AddHealthChecks();
            #endregion

            var app = builder.Build();
            await app.SeedDatabaseAsync();

            #region Configure the HTTP request pipeline.
            app.UseMiddleware<CustomExceptionHandlerMiddleWare>();
            app.UseCors("CorsPolicy");

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapHub<ChatHub>("/chatHub");
            app.MapHub<TechnicianHub>("/technicianHub")
               .RequireAuthorization(new AuthorizeAttribute { Roles = "Technician" });

            app.MapHealthChecks("/health");

            app.MapHub<ClientHub>("/clientHub")
               .RequireAuthorization(new AuthorizeAttribute { Roles = "Client" });
            app.MapControllers();
            #endregion

            app.Run();
        }
    }
}
