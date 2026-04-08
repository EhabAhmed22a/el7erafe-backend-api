using System;
using DomainLayer.Contracts;
using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;
using Persistance.Databases;

namespace Repository
{
    public class RatingRepository(ApplicationDbContext dbcontext) : IRatingRepository
    {
        public async Task<bool> HasRatingAlreadyAsync(int reservationId)
        {
            return await dbcontext.Ratings.AnyAsync(r => r.ReservationId == reservationId);
        }

        public async Task<decimal> AddRatingAndCalculateAverageAsync(Rating newRating, int technicianId)
        {
            await dbcontext.Ratings.AddAsync(newRating);
            await dbcontext.SaveChangesAsync();

            decimal average = await dbcontext.Ratings
                .Where(r => r.Reservation.Offer.TechnicianId == technicianId)
                .AverageAsync(r => (decimal)r.Value);

            return Math.Round(average, 2);
        }
    }
}