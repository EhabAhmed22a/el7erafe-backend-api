using DomainLayer.Models;

namespace DomainLayer.Contracts
{
    public interface INotificationRepository
    {
        Task AddAsync(Notification notification);
        Task<List<Notification>> GetUserNotificationsAsync(string userId);
    }
}
