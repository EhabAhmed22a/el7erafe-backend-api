using el7erafe.Web.CustomMiddleWares;
using el7erafe.Web.Extensions;
using el7erafe.Web.Mapper;
using Persistance;

namespace el7erafe.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region Add services to the container.

            builder.Services.AddControllers();

            #region Swagger Setup
            builder.Services.AddOpenApi();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "api.xml"))
            );
            #endregion

            //builder.Services.AddAutoMapper(typeof(MapperProfile));
            builder.Services.AddPersistanceServices(builder.Configuration); 
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
            app.UseStaticFiles();
            //app.UseAuthorization();


            app.MapControllers(); 
            #endregion

            app.Run();
        }
    }
}
