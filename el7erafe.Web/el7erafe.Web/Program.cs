using el7erafe.Web.CustomMiddleWares;
using el7erafe.Web.Extensions;
using el7erafe.Web.Mapper;
using Microsoft.Extensions.DependencyInjection;
using Persistance;
using Serilog;

namespace el7erafe.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            #region Add services to the container.
            builder.Services.AddPersistanceServices(builder.Configuration);
            #endregion

            builder.Services.AddAutoMapper(typeof(MapperProfile));

            #region Swagger Setup
            builder.Services.AddOpenApi();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "api.xml"))
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
            });
            #endregion

            var app = builder.Build();

            await app.SeedDatabaseAsync();

            #region Configure the HTTP request pipeline.
            app.UseMiddleware<CustomExceptionHandlerMiddleWare>();
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            //app.UseAuthorization();


            app.MapControllers(); 
            #endregion

            app.Run();
        }
    }
}
