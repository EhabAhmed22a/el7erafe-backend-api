using DomainLayer.Contracts;
using DomainLayer.Contracts.ChatModule;
using DomainLayer.Models.ChatModule;
using DomainLayer.Models.ChatModule.Enums;
using DomainLayer.Models.IdentityModule;
using Microsoft.AspNetCore.Identity;
using ServiceAbstraction.Chat;
using Shared.DataTransferObject.ChatDTOs;

namespace Service.Chat
{
    public class ChatService(IChatRepository _chatRepository,
                             IBlobStorageRepository blobStorageRepository,
                             IClientRepository clientRepository,
                             ITechnicianRepository technicianRepository,
                             UserManager<ApplicationUser> _userManager) : IChatService
    {
        public async Task<ChatDto> GetOrCreateChatAsync(string user1Id, string user2Id)
        {
            var user1 = await _userManager.FindByIdAsync(user1Id);
            var roles = await _userManager.GetRolesAsync(user1);

            string clientId;
            string technicianId;

            if (roles.Contains("Client"))
            {
                clientId = user1Id;
                technicianId = user2Id;
            }
            else
            {
                clientId = user2Id;
                technicianId = user1Id;
            }

            var chat = await _chatRepository.GetOrCreateChatAsync(clientId, technicianId);

            return new ChatDto
            {
                Id = chat.Id,
                ClientId = chat.ClientId,
                TechnicianId = chat.TechnicianId
            };
        }

        public async Task<ChatDto?> GetChatByIdAsync(int chatId)
        {
            var chat = await _chatRepository.GetChatByIdAsync(chatId);
            if (chat == null)
                return null;

            return new ChatDto
            {
                Id = chat.Id,
                ClientId = chat.ClientId,
                TechnicianId = chat.TechnicianId
            };
        }

        public async Task<MessageDto> SendMessageAsync(SendMessageDto messageDto, int chatId, string senderId)
        {

            var domainMessage = new Message
            {
                ChatId = chatId,
                SenderId = senderId,
                ReceiverId = messageDto.ReceiverId,
                Content = messageDto.Content,
                Type = MessageType.Text,
                CreatedAt = DateTime.UtcNow,
                Status = MessageStatus.Sent,
            };

            // Persist and get the created message with its Id and timestamps
            var created = await _chatRepository.AddMessageAsync(domainMessage);

            // Map domain Message -> MessageDto
            return new MessageDto
            {
                Id = created.Id,
                ChatId = created.ChatId,
                SenderId = created.SenderId,
                ReceiverId = created.ReceiverId,
                Content = created.Content,
                CreatedAt = created.CreatedAt,
                IsRead = MessageStatus.Sent.ToString() 
            };
        }

        public async Task<IEnumerable<MessageDto>> GetChatHistoryAsync(int chatId, int page = 1, int pageSize = 50)
        {
            var messages = await _chatRepository.GetChatMessagesAsync(chatId, page, pageSize);
            return messages.Select(message => new MessageDto
            {
                Id = message.Id,
                ChatId = message.ChatId,
                SenderId = message.SenderId,
                ReceiverId = message.ReceiverId,
                Content = message.Content,
                CreatedAt = message.CreatedAt,
                IsRead = message.Status.ToString()
            });
        }

        public async Task MarkMessagesAsReadAsync(int chatId, string userId)
        {
            await _chatRepository.MarkMessagesAsReadAsync(chatId, userId);
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _chatRepository.GetUnreadCountAsync(userId);
        }

        public async Task AnonymizeUserDataAsync(string userId, string deletedMarker)
        {
            await _chatRepository.AnonymizeUserChatsAsync(userId, deletedMarker);
        }

        public async Task<IEnumerable<InboxConversationDto>> GetInboxAsync(string userId)
        {
            var chats = await _chatRepository.GetUserChatsWithDetailsAsync(userId);

            var inbox = new List<InboxConversationDto>();

            foreach (var chat in chats)
            {
                var isClient = chat.ClientId == userId;

                string receiverId;
                string receiverName;
                string? receiverImage = null;

                if (isClient)
                {
                    var technician = await technicianRepository.GetByUserIdAsync(chat.TechnicianId);
                    if (technician == null)
                        continue;

                    receiverId = technician.UserId;
                    receiverName = technician.Name;

                    if (!string.IsNullOrEmpty(technician.ProfilePictureURL))
                    {
                        receiverImage = await blobStorageRepository
                            .GetBlobUrlWithSasTokenAsync("technician-documents", technician.ProfilePictureURL);
                    }
                }
                else
                {
                    var client = await clientRepository.GetByUserIdAsync(chat.ClientId);
                    if (client == null)
                        continue;

                    receiverId = client.UserId;
                    receiverName = client.Name;

                    if (!string.IsNullOrEmpty(client.ImageURL))
                    {
                        receiverImage = await blobStorageRepository
                            .GetBlobUrlWithSasTokenAsync("client-profilepics", client.ImageURL);
                    }
                }

                var lastMessage = chat.Messages
                    .Where(m => !m.IsDeleted)
                    .OrderByDescending(m => m.CreatedAt)
                    .FirstOrDefault();

                var unreadCount = chat.Messages
                    .Count(m => m.ReceiverId == userId && m.Status != MessageStatus.Read && !m.IsDeleted);

                inbox.Add(new InboxConversationDto
                {
                    ChatId = chat.Id,
                    ReceiverId = receiverId,
                    ReceiverName = receiverName,
                    ReceiverImage = receiverImage,
                    LastMessageContent = lastMessage?.Content,
                    LastMessageTime = lastMessage?.CreatedAt,
                    IsLastMessageFromMe = lastMessage?.SenderId == userId,
                    UnreadCount = unreadCount
                });
            }

            return inbox
                .OrderByDescending(x => x.LastMessageTime)
                .ToList();
        }

        public async Task UpdateMessageStatusAsync(int messageId, MessageStatus newStatus)
        {
            var message = await _chatRepository.GetMessageByIdAsync(messageId);
            if (message == null)
                return;

            message.Status = newStatus;
            await _chatRepository.UpdateMessageAsync(message);
        }
    }
}
