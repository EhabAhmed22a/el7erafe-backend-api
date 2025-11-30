using DomainLayer.Contracts;
using DomainLayer.Models.IdentityModule;
using DomainLayer.Models.IdentityModule.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Persistance.Databases;
using System.Text.Json;

namespace Persistance
{
    public class DataSeeding(ApplicationDbContext _dbContext,IWebHostEnvironment _webHostEnvironment, ILogger<DataSeeding> logger,
        RoleManager<IdentityRole> _roleManager, UserManager<ApplicationUser> userManager) : IDataSeeding
    {
        private readonly string SeedFilePath = Path.Combine(_webHostEnvironment.ContentRootPath, "data/message.txt");

        public async Task IdentityDataSeedingAsync()
        {
            try
            {
                // Apply pending migrations if any
                var pending = await _dbContext.Database.GetPendingMigrationsAsync();
                if (pending != null && pending.Any())
                {
                    await _dbContext.Database.MigrateAsync();
                }

                // Seed roles
                if (!await _roleManager.Roles.AnyAsync())
                {
                    await _roleManager.CreateAsync(new IdentityRole("Client"));
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));
                    await _roleManager.CreateAsync(new IdentityRole("Technician"));
                }
                if (!await _dbContext.TechnicianServices.AnyAsync())
                {
                    _dbContext.TechnicianServices.AddRange(
                        new TechnicianService { NameAr = "سباك" },
                        new TechnicianService { NameAr = "كهربائي" },
                        new TechnicianService { NameAr = "نجار" }
                    );
                }
                if (!await _dbContext.Admins.AnyAsync())
                {
                    var user = new ApplicationUser
                    {
                        UserName = "ahmedSalah",
                        Email = "temp@temp.com",
                        EmailConfirmed = true,
                        PhoneNumber = "01201884828", 
                        UserType = UserTypeEnum.Admin
                    };
                    var result = await userManager.CreateAsync(user, "ahmedSalah123$");
                    if (result.Succeeded)
                    {
                        await _dbContext.Admins.AddAsync(new Admin()
                        {
                            Name = "Ahmed Salah",
                            UserId = user.Id,
                        });
                    }
                }
                // Save any role changes
                await _dbContext.SaveChangesAsync();

                // Seed governorates & cities only if table empty (idempotent)
                if (await _dbContext.Governorates.AnyAsync())
                    return;

                if (!File.Exists(SeedFilePath))
                {
                    logger.LogInformation($"Seed file not found: {SeedFilePath}");
                    return;
                }

                var json = await File.ReadAllTextAsync(SeedFilePath);

                // Parse the JSON as a document and read the top-level "governorates" array
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (!root.TryGetProperty("governorates", out var govArray) || govArray.ValueKind != JsonValueKind.Array)
                {
                    Console.WriteLine("Seed JSON does not contain a top-level 'governorates' array.");
                    return;
                }

                using var tx = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    foreach (var govEl in govArray.EnumerateArray())
                    {
                        var nameEn = govEl.GetProperty("name_en").GetString()?.Trim() ?? string.Empty;
                        var nameAr = govEl.GetProperty("name_ar").GetString()?.Trim() ?? string.Empty;

                        // find existing governorate by either name
                        var existingGov = await _dbContext.Governorates
                            .FirstOrDefaultAsync(x => x.NameEn == nameEn || x.NameAr == nameAr);

                        if (existingGov == null)
                        {
                            existingGov = new Governorate { NameEn = nameEn, NameAr = nameAr };
                            _dbContext.Governorates.Add(existingGov);
                            await _dbContext.SaveChangesAsync(); // ensure Id is generated
                        }

                        if (govEl.TryGetProperty("cities", out var citiesEl) && citiesEl.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var cityEl in citiesEl.EnumerateArray())
                            {
                                var cityEn = cityEl.GetProperty("name_en").GetString()?.Trim() ?? string.Empty;
                                var cityAr = cityEl.GetProperty("name_ar").GetString()?.Trim() ?? string.Empty;

                                var existsCity = await _dbContext.Cities
                                    .FirstOrDefaultAsync(x =>
                                        x.GovernorateId == existingGov.Id &&
                                        (x.NameEn == cityEn || x.NameAr == cityAr));

                                if (existsCity == null)
                                {
                                    _dbContext.Cities.Add(new City
                                    {
                                        NameEn = cityEn,
                                        NameAr = cityAr,
                                        GovernorateId = existingGov.Id
                                    });
                                }
                            }

                            // save cities for this governorate
                            await _dbContext.SaveChangesAsync();
                        }
                    }

                    await tx.CommitAsync();
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
