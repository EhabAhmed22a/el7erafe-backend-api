using DomainLayer.Contracts;
using DomainLayer.Exceptions;
using DomainLayer.Models;
using Service.Helpers;
using ServiceAbstraction;
using Shared.DataTransferObject.OffersDTOs;

namespace Service
{
    public class OfferService(
        IOffersRepository offersRepository,
        IServiceRequestRepository serviceRequestRepository,
        ITechnicianRepository technicianRepository,
        IBlobStorageRepository blobStorageRepository) : IOfferService
    {
        public async Task<OfferResultDto> MakeOfferAsync(MakeOfferDto dto, string technicianUserId)
        {
            if (dto.FromTime >= dto.ToTime)
                throw new ArgumentException("يجب أن يكون وقت البداية قبل وقت النهاية.");

            var technician = await technicianRepository.GetByUserIdAsync(technicianUserId);

            if (technician == null)
                throw new TechNotFoundException(technicianUserId);

            var request = await serviceRequestRepository.GetServiceById(dto.RequestId);

            if (request == null)
                throw new KeyNotFoundException("طلب الخدمة غير موجود.");

            var alreadyOffered = await offersRepository.HasTechnicianAlreadyOffered(technician.Id, dto.RequestId);

            if (alreadyOffered)
                throw new InvalidOperationException("لقد قمت بتقديم عرض على هذا الطلب بالفعل.");

            var hasConflict = await offersRepository.HasTimeConflict(
                technician.Id,
                dto.FromTime,
                dto.ToTime,
                request.ServiceDate,
                dto.NumberOfDays);

            if (hasConflict)
                throw new InvalidOperationException("وقت العرض يتعارض مع حجز مؤكد لديك.");

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

            await offersRepository.AddOfferAsync(offer);

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
                TechTimeInterval = HelperClass.FormatArabicTimeInterval(offer.WorkFrom, offer.WorkTo),
                NumberOfDays = offer.NumberOfDays,
                ClientId = request.ClientId
            };
        }
    }
}
