using DomainLayer.Models;

namespace DomainLayer.Contracts
{
    public interface IOffersRepository
    {
        Task<Offer> AddOfferAsync(Offer offer);

        Task<bool> HasTechnicianAlreadyOffered(int technicianId, int requestId);

        Task<bool> HasTimeConflict(int technicianId, TimeSpan fromTime, TimeSpan toTime, DateTime serviceDate, int? numberOfDays);
    }
}
