using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Persistance.Databases; 
using DomainLayer.Models;    

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        // 1. Grab the connection string we defined in local.settings.json
        var connectionString = context.Configuration["DefaultConnection"];

        // 2. Register ApplicationDbContext with the built-in Dependency Injection pool
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));
    })
    .Build();

host.Run();