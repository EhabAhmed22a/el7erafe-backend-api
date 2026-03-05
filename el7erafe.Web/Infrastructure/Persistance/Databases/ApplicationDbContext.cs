using DomainLayer.Models;
using DomainLayer.Models.ChatModule;
using DomainLayer.Models.ChatModule.Enums;
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
        public DbSet<ServiceRequest> ServiceRequests { get; set; }
        public DbSet<UserConnection> UserConnections { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Message> Messages { get; set; }

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
            builder.Entity<ServiceRequest>().ToTable("ServiceRequests");
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

                entity.Property(t => t.Rating)
                      .HasColumnType("decimal(3,2)")
                      .HasDefaultValue(0);
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

            builder.Entity<UserConnection>(entity =>
            {
                entity.ToTable("UserConnections");

                // Primary Key
                entity.HasKey(e => e.Id);

                // Indexes
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.ConnectionId)
                    .IsUnique();  // ConnectionId should be unique

                // Relationship with User
                entity.HasOne(e => e.User)
                    .WithMany(u => u.UserConnections)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Properties
                entity.Property(e => e.ConnectionId)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.ConnectedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
            });

            builder.Entity<Chat>(entity =>
            {
                entity.ToTable("Chats");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(e => e.ClientId);
                entity.HasIndex(e => e.TechnicianId);

                entity.HasIndex(e => new { e.ClientId, e.TechnicianId })
                      .IsUnique();

                entity.HasOne(e => e.Client)
                      .WithMany(c => c.Chats)
                      .HasForeignKey(e => e.ClientId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Technician)
                      .WithMany(t => t.Chats)
                      .HasForeignKey(e => e.TechnicianId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Message>(entity =>
            {
                entity.ToTable("Messages");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Content)
                      .IsRequired()
                      .HasMaxLength(4000);

                entity.Property(e => e.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.IsDeleted)
                      .HasDefaultValue(false);

                entity.HasIndex(e => e.ChatId);
                entity.HasIndex(e => e.SenderId);

                entity.HasIndex(e => new { e.ChatId, e.CreatedAt });

                entity.HasOne(e => e.Chat)
                      .WithMany(c => c.Messages)
                      .HasForeignKey(e => e.ChatId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Sender)
                      .WithMany(u => u.SentMessages)
                      .HasForeignKey(e => e.SenderId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}