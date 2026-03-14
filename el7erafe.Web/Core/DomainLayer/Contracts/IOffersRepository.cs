using DomainLayer.Models;

namespace DomainLayer.Contracts
{
    public interface IOffersRepository
    {
        Task<Offer> AddOfferAsync(Offer offer);

        Task<bool> HasTechnicianAlreadyOffered(int technicianId, int requestId);
        Task<IEnumerable<Offer>> GetValidQuickOffersForClientAsync(int serReqId, int clientId);
        Task<IEnumerable<Offer>> GetValidTechOfferForClientsAsync(int serReqId, int clientId);
        Task<bool> HasTimeConflict(int technicianId, TimeOnly fromTime, TimeOnly toTime, DateOnly serviceDate, int? numberOfDays);
    }
}
