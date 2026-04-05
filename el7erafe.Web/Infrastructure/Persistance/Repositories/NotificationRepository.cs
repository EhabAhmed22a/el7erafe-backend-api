using DomainLayer.Contracts;
using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;
using Persistance.Databases;

namespace Persistance.Repositories
{
    internal class NotificationRepository(ApplicationDbContext dbContext) : INotificationRepository
    {
        public async Task AddAsync(Notification notification)
        {
            await dbContext.Notifications.AddAsync(notification);
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(string userId)
        {
            return await dbContext.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }
    }
}
