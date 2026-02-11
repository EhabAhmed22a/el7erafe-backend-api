using Azure.Identity;
using Azure.Storage.Blobs;
using el7erafe.Web.CustomMiddleWares;
using el7erafe.Web.Extensions;
using el7erafe.Web.Filters;
using Persistance;
using Persistance.Databases;
using Serilog;
using Service;
using Service.Email;
using ServiceAbstraction;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace el7erafe.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

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
                         "https://www.7otob3den.com")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
            });

            #region Json Options
            builder.Services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
            });
            #endregion

            #region Add services to the container.
            builder.Services.AddPersistanceServices(builder.Configuration);
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
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();
            #endregion

            app.Run();
        }
    }
}
