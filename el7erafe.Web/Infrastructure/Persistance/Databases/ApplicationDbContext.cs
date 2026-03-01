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
                // Table name
                entity.ToTable("Chats");

                // Primary Key
                entity.HasKey(e => e.Id);

                // Properties
                entity.Property(e => e.ClientId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(e => e.TechnicianId)
                    .IsRequired()
                    .HasMaxLength(450);

                // Indexes for fast lookups
                entity.HasIndex(e => e.ClientId)
                    .HasDatabaseName("IX_Chats_ClientId");

                entity.HasIndex(e => e.TechnicianId)
                    .HasDatabaseName("IX_Chats_TechnicianId");

                // Unique constraint to prevent duplicate chats between same client/technician
                entity.HasIndex(e => new { e.ClientId, e.TechnicianId })
                    .IsUnique()
                    .HasDatabaseName("IX_Chats_Client_Technician");

                // Relationships
                entity.HasOne(e => e.Client)
                    .WithMany(u => u.ClientChats)
                    .HasForeignKey(e => e.ClientId)
                    .OnDelete(DeleteBehavior.Restrict); 

                entity.HasOne(e => e.Technician)
                    .WithMany(u => u.TechnicianChats)
                    .HasForeignKey(e => e.TechnicianId)
                    .OnDelete(DeleteBehavior.Restrict); 
            });

            builder.Entity<Message>(entity =>
            {
                // Table name
                entity.ToTable("Messages");

                // Primary Key
                entity.HasKey(e => e.Id);

                // Properties
                entity.Property(e => e.ChatId)
                    .IsRequired();

                entity.Property(e => e.SenderId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(e => e.ReceiverId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(e => e.Content)
                    .IsRequired()
                    .HasMaxLength(4000); 

                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.IsDeleted)
                    .IsRequired()
                    .HasDefaultValue(false);

                // Indexes for performance
                entity.HasIndex(e => e.ChatId)
                    .HasDatabaseName("IX_Messages_ChatId");

                entity.HasIndex(e => e.SenderId)
                    .HasDatabaseName("IX_Messages_SenderId");

                entity.HasIndex(e => e.ReceiverId)
                    .HasDatabaseName("IX_Messages_ReceiverId");

                // Composite index for chat history ordering
                entity.HasIndex(e => new { e.ChatId, e.CreatedAt })
                    .HasDatabaseName("IX_Messages_ChatId_CreatedAt");

                // Index for unread counts
                entity.HasIndex(e => new { e.ReceiverId, e.Status, e.ChatId })
                    .HasDatabaseName("IX_Messages_ReceiverId_IsRead_ChatId");

                // Relationships
                entity.HasOne(e => e.Chat)
                    .WithMany(c => c.Messages)
                    .HasForeignKey(e => e.ChatId)
                    .OnDelete(DeleteBehavior.Cascade); // If chat is deleted, delete all its messages

                entity.HasOne(e => e.Sender)
                    .WithMany(u => u.SentMessages)
                    .HasForeignKey(e => e.SenderId)
                    .OnDelete(DeleteBehavior.Restrict); // Don't delete messages if sender is deleted

                entity.HasOne(e => e.Receiver)
                    .WithMany(u => u.ReceivedMessages)
                    .HasForeignKey(e => e.ReceiverId)
                    .OnDelete(DeleteBehavior.Restrict); // Don't delete messages if receiver is deleted
            });
        }
    }
}