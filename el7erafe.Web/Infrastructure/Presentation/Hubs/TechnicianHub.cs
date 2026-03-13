
using DomainLayer.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ServiceAbstraction;

namespace Presentation.Hubs
{
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "Technician")]
    public class TechnicianHub(ITechnicianRealTimeService technicianRealTimeService): Hub
    {
        public override async Task OnConnectedAsync()
        {
            if (string.IsNullOrEmpty(Context.UserIdentifier))
            {
                Context.Abort();
                throw new UserNotFoundException("المستخدم غير موجود");
            }
            await technicianRealTimeService.AddUserConnectionAsync(Context.UserIdentifier, Context.ConnectionId);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await technicianRealTimeService.RemoveConnectionAsync(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
