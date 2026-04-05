using DomainLayer.Models.IdentityModule;
using Microsoft.AspNetCore.Identity;
using ServiceAbstraction;

namespace Service
{
    public class UserService(UserManager<ApplicationUser> userManager) : IUserService
    {
        public async Task DeleteFcmTokenAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
                throw new Exception("User not found");

            user.FcmToken = null;

            await userManager.UpdateAsync(user);
        }

        public async Task SaveFcmTokenAsync(string userId, string token)
        {
            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
                throw new Exception("User not found");

            user.FcmToken = token;

            await userManager.UpdateAsync(user);
        }

        public async Task SetNotificationStatus(string userId, bool enabled)
        {
            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
                throw new Exception("User not found");

            if (user.NotificationsEnabled == enabled)
                return;

            user.NotificationsEnabled = enabled;

            await userManager.UpdateAsync(user);
        }
    }
}