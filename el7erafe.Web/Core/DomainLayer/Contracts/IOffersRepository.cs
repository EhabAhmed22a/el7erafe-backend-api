using DomainLayer.Models;

namespace DomainLayer.Contracts
{
    public interface IOffersRepository
    {
        Task<Offer> AddOfferAsync(Offer offer);
        Task<Offer?> GetByIdAsync(int offerId);
        Task<bool> HasTechnicianAlreadyOffered(int technicianId, int requestId);
        Task<IEnumerable<Offer>> GetValidOffersForClientAsync(int serReqId, int clientId, bool isQuick);
        Task<bool> HasTimeConflict(int technicianId, TimeOnly fromTime, TimeOnly toTime, DateOnly serviceDate, int? numberOfDays);
        Task<List<Offer>> GetPendingOffersForTechAsync(int technicianId);
        Task<List<string>> GetTechniciansUserIdByRequestId(int requestId);
    }
}
