using DomainLayer.Contracts;
using DomainLayer.Contracts.ChatModule;
using DomainLayer.Exceptions;
using DomainLayer.Models.ChatModule;
using DomainLayer.Models.ChatModule.Enums;
using DomainLayer.Models.IdentityModule;
using DomainLayer.Models.IdentityModule.Enums;
using Microsoft.AspNetCore.Identity;
using ServiceAbstraction.Chat;
using Shared.DataTransferObject.ChatDTOs;

namespace Service.Chat
{
    public class ChatService(IChatRepository _chatRepository,
                             IUserConnectionRepository userConnectionRepository,
                             IBlobStorageRepository blobStorageRepository,
                             IClientRepository clientRepository,
                             ITechnicianRepository technicianRepository,
                             IReservationRepository reservationRepository) : IChatService
    {
        public async Task<UserConnection> AddUserConnectionAsync(string userId, string connectionId)
        {
            try
            {
                return await userConnectionRepository.AddConnectionAsync(userId, connectionId, HubType.Chat);
            }
            catch
            {
                throw new TechnicalException();
            }
        }
        public async Task RemoveConnectionAsync(string connectionId)
        {
            try
            {
                await userConnectionRepository.RemoveConnectionAsync(connectionId);
            }
            catch
            {
                throw new TechnicalException();
            }
        }
        public async Task<List<string>> GetUserChatConnectionsAsync(string userId)
        {
            return await userConnectionRepository.GetUserConnectionsByTypeAsync(userId, HubType.Chat);
        }
        public async Task<ChatDto> InitChatAsync(string userId, int reservationId)
        {
            var reservation = await reservationRepository.GetByIdWithDetailsAsync(reservationId);

            if (reservation == null)
                throw new KeyNotFoundException("Reservation not found");

            var clientEntityId = reservation.Offer.ServiceRequest.ClientId;
            var technicianEntityId = reservation.Offer.TechnicianId;

            var client = await clientRepository.GetByIdAsync(clientEntityId);
            var technician = await technicianRepository.GetByIdAsync(technicianEntityId);

            if (client == null || technician == null)
                throw new Exception("Client or Technician not found");

            var clientUserId = client.UserId;
            var technicianUserId = technician.UserId;

            if (clientUserId != userId && technicianUserId != userId)
                throw new UnauthorizedAccessException();

            if (reservation.Status != ReservationStatus.Confirmed &&
                reservation.Status != ReservationStatus.InProgress &&
                reservation.Status != ReservationStatus.InPayment)
            {
                throw new Exception("Chat not allowed for this reservation status");
            }

            var chat = await _chatRepository.GetOrCreateChatAsync(reservationId,clientUserId, technicianUserId);
            if (chat.IsHidden)
            {
                chat.IsHidden = false;
                await _chatRepository.UpdateChatAsync(chat);
            }

            return new ChatDto
            {
                Id = chat.Id
            };
        }

        public async Task<MessageDto> SendMessageAsync(SendMessageDto messageDto, string senderId)
        {
            // 1️ Get chat
            var chat = await _chatRepository.GetChatByIdAsync(messageDto.ChatId);

            if (chat == null)
                throw new Exception("Chat not found");

            // 2️ Validate sender belongs to chat
            if (chat.ClientId != senderId && chat.TechnicianId != senderId)
                throw new UnauthorizedAccessException();

            // 3️ Determine receiver 
            string receiverId;

            if (chat.ClientId == senderId)
                receiverId = chat.TechnicianId;
            else
                receiverId = chat.ClientId;

            // 4️ Parse message type
            var messageType = ParseMessageType(messageDto.MessageType);

            // 5️ Create message
            var message = new Message
            {
                ChatId = chat.Id,
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = messageDto.Content,
                Type = messageType,
                CreatedAt = DateTime.UtcNow,
                Status = MessageStatus.Sent
            };

            // 6️ Save
            var created = await _chatRepository.AddMessageAsync(message);

            // 7️ Return DTO
            return new MessageDto
            {
                Id = created.Id,
                ChatId = created.ChatId,
                SenderId = created.SenderId,
                ReceiverId = created.ReceiverId,
                Content = created.Content,
                CreatedAt = created.CreatedAt,
                MessageType = created.Type.ToString(),
                MessageStatus = created.Status.ToString()
            };
        }

        public async Task<IEnumerable<MessageDto>> GetChatHistoryAsync(string userId, int chatId, int page = 1,int pageSize = 50)
        {
            // 1️ Validate chat ownership
            var chat = await _chatRepository.GetChatByIdAsync(chatId);

            if (chat == null || (chat.ClientId != userId && chat.TechnicianId != userId))
                throw new UnauthorizedAccessException();

            // 2️ Get messages 
            var messages = await _chatRepository.GetChatMessagesAsync(chatId, page, pageSize);

            // 3️ Map to DTO
            return messages.Select(message => new MessageDto
            {
                Id = message.Id,
                ChatId = message.ChatId,
                SenderId = message.SenderId,
                ReceiverId = message.ReceiverId,
                Content = message.Content,
                CreatedAt = message.CreatedAt,
                MessageType = message.Type.ToString(),
                MessageStatus = message.Status.ToString()
            });
        }

        public async Task<(List<int> UpdatedMessageIds, string OtherUserId)> MarkMessagesAsReadCoreAsync(int chatId, string userId)
        {
            // 1️ Get chat
            var chat = await _chatRepository.GetChatByIdAsync(chatId);

            if (chat == null || (chat.ClientId != userId && chat.TechnicianId != userId))
                throw new UnauthorizedAccessException();

            // 2️ Determine other user
            string otherUserId = chat.ClientId == userId ? chat.TechnicianId : chat.ClientId;

            // 3️ Mark messages as read
            var updatedMessageIds = await _chatRepository.MarkMessagesAsReadAsync(chatId, userId);

            return (updatedMessageIds, otherUserId);
        }
        public async Task<Dictionary<string, List<int>>> MarkAllMessagesAsDeliveredAsync(string userId)
        {
            return await _chatRepository.MarkAllMessagesAsDeliveredAsync(userId);
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
                    MessageStatus = lastMessage?.Status.ToString() ?? "UnKnown",
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

        private MessageType ParseMessageType(string type)
        {
            if (!Enum.TryParse<MessageType>(type, true, out var result))
                throw new Exception("Invalid message type");

            return result;
        }

        public async Task<bool> IsUserOnline(string userId)
        {
            return await userConnectionRepository.IsUserOnlineAsync(userId, HubType.Chat);
        }

        public async Task<(string Name, string? ImageUrl)> GetUserBasicInfoAsync(string userId)
        {
            // Try Technician first
            var technician = await technicianRepository.GetByUserIdAsync(userId);
            if (technician != null)
            {
                string? image = null;

                if (!string.IsNullOrEmpty(technician.ProfilePictureURL))
                {
                    image = await blobStorageRepository.GetBlobUrlWithSasTokenAsync(
                        "technician-documents",
                        technician.ProfilePictureURL);
                }

                return (technician.Name, image);
            }

            // Try Client
            var client = await clientRepository.GetByUserIdAsync(userId);
            if (client != null)
            {
                string? image = null;

                if (!string.IsNullOrEmpty(client.ImageURL))
                {
                    image = await blobStorageRepository.GetBlobUrlWithSasTokenAsync(
                        "client-profilepics",
                        client.ImageURL);
                }

                return (client.Name, image);
            }

            throw new Exception("User not found");
        }
    }
}
