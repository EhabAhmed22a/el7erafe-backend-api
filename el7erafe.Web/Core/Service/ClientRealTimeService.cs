using DomainLayer.Contracts.ChatModule;
using DomainLayer.Exceptions;
using DomainLayer.Models.ChatModule;
using DomainLayer.Models.ChatModule.Enums;
using ServiceAbstraction;

namespace Service
{
    public class ClientRealTimeService(IUserConnectionRepository userConnectionRepository) : IClientRealTimeService
    {
        public async Task<UserConnection> AddUserConnectionAsync(string userId, string connectionId)
        {
            try
            {
                return await userConnectionRepository.AddConnectionAsync(userId, connectionId, HubType.Client);
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

    }
}
