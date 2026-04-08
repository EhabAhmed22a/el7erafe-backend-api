using DomainLayer.Models;

namespace DomainLayer.Contracts
{
    public interface IRatingRepository
    {
        Task<bool> HasRatingAlreadyAsync(int reservationId);
        Task<decimal> AddRatingAndCalculateAverageAsync(Rating newRating, int technicianId);
    }
}