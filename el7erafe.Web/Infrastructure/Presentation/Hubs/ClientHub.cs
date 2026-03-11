using DomainLayer.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ServiceAbstraction;

namespace Presentation.Hubs
{
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "Client")]
    public class ClientHub(IClientRealTimeService clientRealTimeService): Hub
    {
        public override async Task OnConnectedAsync()
        {
            if (string.IsNullOrEmpty(Context.UserIdentifier))
            {
                Context.Abort();
                throw new UserNotFoundException("المستخدم غير موجود");
            }
            await clientRealTimeService.AddUserConnectionAsync(Context.UserIdentifier, Context.ConnectionId);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await clientRealTimeService.RemoveConnectionAsync(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task RequestMyPendingData()
        {
            if (string.IsNullOrEmpty(Context.UserIdentifier))
            {
                Context.Abort();
                throw new UserNotFoundException("المستخدم غير موجود");
            }
            var data = await clientRealTimeService.GetServiceRequestsAsync(Context.UserIdentifier);
            await Clients.Caller.SendAsync("ReceivePendingRequests", data);
        }
    }
}
