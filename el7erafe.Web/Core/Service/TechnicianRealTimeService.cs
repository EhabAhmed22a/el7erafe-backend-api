
using DomainLayer.Contracts;
using DomainLayer.Contracts.ChatModule;
using DomainLayer.Exceptions;
using DomainLayer.Models.ChatModule;
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
                return await userConnectionRepository.AddConnectionAsync(userId, connectionId);
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
