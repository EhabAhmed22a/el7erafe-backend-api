using DomainLayer.Models;

namespace DomainLayer.Contracts
{
    public interface IOffersRepository
    {
        Task<Offer> AddOfferAsync(Offer offer);

        Task<bool> HasTechnicianAlreadyOffered(int technicianId, int requestId);
        Task<IEnumerable<Offer>> GetValidOffersForClientAsync(int serReqId, int clientId, bool isQuick);
        Task<bool> HasTimeConflict(int technicianId, TimeOnly fromTime, TimeOnly toTime, DateOnly serviceDate, int? numberOfDays);
    }
}
