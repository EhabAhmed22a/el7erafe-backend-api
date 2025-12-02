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

        public DbSet<UserToken> UserTokens { get; set; } = default!;
        public DbSet<Rejection> Rejections { get; set; } = default!;
        public DbSet<Admin> Admins { get; set; } = default!;
        public DbSet<BlockedUser> BlockedUsers { get; set; } = default!;
        public DbSet<RejectionComment> rejectionComments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<Technician>().ToTable("Technicians");
            builder.Entity<Client>().ToTable("Clients");
            builder.Entity<BlockedUser>().ToTable("BlockedUsers");
            builder.Entity<IdentityRole>().ToTable("Roles");
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            builder.Entity<Governorate>().ToTable("Governorates");
            builder.Entity<TechnicianService>().ToTable("TechnicianServices");
            builder.Entity<City>().ToTable("Cities");
            builder.Entity<Admin>().ToTable("Admins");
            builder.Entity<RejectionComment>().ToTable("RejectionComments");
            builder.Ignore<IdentityUserClaim<string>>();
            builder.Ignore<IdentityUserToken<string>>();
            builder.Ignore<IdentityUserLogin<string>>();
            builder.Ignore<IdentityRoleClaim<string>>();

            // Configure Technician
            builder.Entity<Technician>(entity =>
            {
                // One-to-One with Rejection
                entity.HasOne(t => t.Rejection)
                      .WithOne(r => r.Technician)
                      .HasForeignKey<Rejection>(r => r.TechnicianId) // Explicit foreign key
                      .OnDelete(DeleteBehavior.Cascade);
            });

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

                // Configure Token column with length 2000
                entity.Property(ut => ut.Token)
                      .HasMaxLength(2000)
                      .IsRequired();

                // Configure enum storage
                entity.Property(ut => ut.Type)
                      .HasConversion<int>();

                // Optional: Configure column names
                entity.Property(ut => ut.Id)
                      .HasColumnName("TokenId");
            });

            builder.Entity<Rejection>(entity =>
            {
                entity.HasKey(r => r.Id);

                entity.Property(r => r.Reason)
                      .HasMaxLength(500)
                      .IsRequired();

                // Index for better performance
                entity.HasIndex(r => r.TechnicianId)
                .IsUnique();
            });

        }
    }
}