using DomainLayer.Models.IdentityModule;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Persistance.Databases
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Governorate> Governorates { get; set; } = default!;
        public DbSet<City> Cities { get; set; } = default!;
        public DbSet<TechnicianService> TechnicianServices { get; set; } = default!;

        public DbSet<UserToken> UserTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<Technician>().ToTable("Technicians");
            builder.Entity<Client>().ToTable("Clients");
            builder.Entity<IdentityRole>().ToTable("Roles");
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            builder.Entity<Governorate>().ToTable("Governorates");
            builder.Entity<TechnicianService>().ToTable("TechnicianServices");
            builder.Entity<City>().ToTable("Cities");
            builder.Ignore<IdentityUserClaim<string>>();
            builder.Ignore<IdentityUserToken<string>>();
            builder.Ignore<IdentityUserLogin<string>>();
            builder.Ignore<IdentityRoleClaim<string>>();

            // Configure UserToken entity
            builder.Entity<UserToken>(entity =>
            {
                // Primary Key
                entity.HasKey(ut => ut.Id);

                // One-to-One relationship with ApplicationUser
                entity.HasOne(ut => ut.User)
                      .WithOne(u => u.UserToken)
                      .HasForeignKey<UserToken>(ut => ut.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Unique constraints
                entity.HasIndex(ut => ut.Token)
                      .IsUnique();

                entity.HasIndex(ut => ut.UserId)
                      .IsUnique(); // Ensures one-to-one

                // Configure enum storage
                entity.Property(ut => ut.Type)
                      .HasConversion<int>();

                // Optional: Configure column names
                entity.Property(ut => ut.Id)
                      .HasColumnName("TokenId");
            });
        }
    }
}