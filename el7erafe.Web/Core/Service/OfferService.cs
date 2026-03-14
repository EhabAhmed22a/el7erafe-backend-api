using DomainLayer.Contracts;
using DomainLayer.Models;
using ServiceAbstraction;
using Shared.DataTransferObject.OffersDTOs;

namespace Service
{
    internal class OfferService(
        IOffersRepository offersRepository,
        IServiceRequestRepository serviceRequestRepository,
        ITechnicianRepository technicianRepository,
        IBlobStorageRepository blobStorageRepository) : IOfferService
    {
        public async Task<OfferResultDto> MakeOfferAsync(MakeOfferDto dto, string technicianUserId)
        {
            if (dto.FromTime >= dto.ToTime)
                throw new Exception("fromTime must be earlier than toTime");

            // 2️⃣ Get technician
            var technician = await technicianRepository.GetByUserIdAsync(technicianUserId);

            if (technician == null)
                throw new Exception("Technician not found");

            // 3️⃣ Get service request
            var request = await serviceRequestRepository.GetServiceById(dto.RequestId);

            if (request == null)
                throw new Exception("Service request not found");

            // 4️⃣ Prevent duplicate offers
            var alreadyOffered = await offersRepository.HasTechnicianAlreadyOffered(technician.Id, dto.RequestId);

            if (alreadyOffered)
                throw new Exception("You already placed an offer on this request");

            // 5️⃣ Check time conflict
            var hasConflict = await offersRepository.HasTimeConflict(
                technician.Id,
                dto.FromTime,
                dto.ToTime,
                request.ServiceDate,
                dto.NumberOfDays);

            if (hasConflict)
                throw new Exception("Offer time conflicts with an existing reservation");

            // 6️⃣ Create offer
            var offer = new Offer
            {
                Fees = dto.Fees,
                SentAt = DateTime.UtcNow,
                ServiceRequestId = dto.RequestId,
                TechnicianId = technician.Id,
                WorkFrom = dto.FromTime,
                WorkTo = dto.ToTime,
                NumberOfDays = dto.NumberOfDays
            };

            // 7️⃣ Save
            await offersRepository.AddOfferAsync(offer);

            // 8️⃣ Return DTO
            return new OfferResultDto
            {
                OfferId = offer.Id,
                RequestId = dto.RequestId,
                Day = request.ServiceDate,
                ServiceType = technician.Service.NameAr,
                TechName = technician.Name,
                TechImage = await blobStorageRepository.GetBlobUrlWithSasTokenAsync("technician-documents",technician.ProfilePictureURL),
                Rate = technician.Rating,
                Fees = offer.Fees,
                FromTime = offer.WorkFrom.Value,
                ToTime = offer.WorkTo.Value,
                NumberOfDays = offer.NumberOfDays,
                ClientId = request.ClientId
            };
        }
    }
}
