using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ServiceAbstraction.Chat;

namespace Service.Hubs
{
    [Authorize]
    public class ChatHub(IChatService chatService) : Hub
    {
    }
}
