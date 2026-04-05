
namespace ServiceAbstraction
{
    public interface IUserService
    {
        Task SaveFcmTokenAsync(string userId, string token);
        Task DeleteFcmTokenAsync(string userId);
        Task SetNotificationStatus(string userId, bool enabled);
    }
}
