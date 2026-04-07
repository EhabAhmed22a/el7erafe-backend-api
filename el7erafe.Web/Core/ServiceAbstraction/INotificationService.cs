using Shared.DataTransferObject.NotificationDTOs;

namespace ServiceAbstraction
{
    public interface INotificationService
    {
        Task SendAsync(string userId, NotificationDto dto);
        Task SendAsync(List<string> userIds, NotificationDto dto);
    }
}
