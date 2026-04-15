using DomainLayer.Contracts;
using DomainLayer.Contracts.ChatModule;
using DomainLayer.Exceptions;
using DomainLayer.Models.ChatModule;
using DomainLayer.Models.ChatModule.Enums;
using ServiceAbstraction;

namespace Service
{
    public class TechnicianRealTimeService(IUserConnectionRepository userConnectionRepository,
        ITechnicianRepository technicianRepository
        ) : ITechnicianRealTimeService
    {
        public async Task<UserConnection> AddUserConnectionAsync(string userId, string connectionId)
        {
            try
            {
                return await userConnectionRepository.AddConnectionAsync(userId, connectionId, HubType.Technician);
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
