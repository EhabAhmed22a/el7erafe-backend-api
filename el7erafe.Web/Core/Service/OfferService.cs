using DomainLayer.Contracts;
using DomainLayer.Exceptions;
using DomainLayer.Models;
using Service.Helpers;
using ServiceAbstraction;
using Shared.DataTransferObject.OffersDTOs;
using DomainLayer.Models.IdentityModule.Enums;

namespace Service
{
    public class OfferService(
        IOffersRepository offersRepository,
        IServiceRequestRepository serviceRequestRepository,
        ITechnicianRepository technicianRepository,
        IBlobStorageRepository blobStorageRepository) : IOfferService
    {
        public async Task<MakeOfferEventResultDto> MakeOfferAsync(MakeOfferDto dto, string technicianUserId)
        {
            if (dto.ToTime == new TimeOnly(0, 0))
            {
                dto.ToTime = new TimeOnly(23, 59);
            }

            if (dto.FromTime >= dto.ToTime)
                throw new ArgumentException("يجب أن يكون وقت البداية قبل وقت النهاية.");

            var technician = await technicianRepository.GetByUserIdAsync(technicianUserId);

            if (technician == null)
                throw new TechNotFoundException(technicianUserId);

            var request = await serviceRequestRepository.GetServiceById(dto.RequestId);

            if (request == null)
                throw new KeyNotFoundException("طلب الخدمة غير موجود.");

            if (request.Status != ServiceReqStatus.Pending)
                throw new InvalidOperationException("لا يمكن تقديم عرض على طلب غير متاح.");

            if (request.TechnicianId != null && request.TechnicianId != technician.Id)
                throw new UnauthorizedAccessException("هذا الطلب موجه لفني آخر.");

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
                NumberOfDays = dto.NumberOfDays,
                Status = OfferStatus.Pending
            };

            await offersRepository.AddOfferAsync(offer);

            var techTimeInterval = HelperClass.FormatArabicTimeInterval(offer.WorkFrom,offer.WorkTo);

            var clientOffer = new OfferResultDto
            {
                OfferId = offer.Id,
                RequestId = dto.RequestId,
                Day = request.ServiceDate,
                ServiceType = technician.Service.NameAr,
                TechName = technician.Name,
                TechImage = await blobStorageRepository.GetBlobUrlWithSasTokenAsync("technician-documents", technician.ProfilePictureURL),
                Rate = technician.Rating,
                Fees = offer.Fees,
                TechTimeInterval = techTimeInterval,
                NumberOfDays = offer.NumberOfDays,
                ClientId = request.ClientId
            };

            List<string> serviceURLs =await blobStorageRepository.GetBlobUrlsWithPrefixAsync("service-requests-images",$"{request.Id}_");

            var techOffer = new PendingOfferDto
            {
                OfferId = offer.Id,

                Description = request.Description,

                ClientName = request.Client.Name,

                ClientImage = string.IsNullOrWhiteSpace(request.Client.ImageURL)? null: await blobStorageRepository.GetBlobUrlWithSasTokenAsync("client-profilepics",request.Client.ImageURL),
                Fees = offer.Fees,
                ServiceType = request.Service.NameAr,
                TechTimeInterval = techTimeInterval,
                Day = request.ServiceDate,
                EndDay = offer.NumberOfDays != null
                                ? request.ServiceDate.AddDays(offer.NumberOfDays.Value - 1)
                                : null,

                Governorate = request.City?.Governorate?.NameAr,
                City = request.City?.NameAr,
                Street = request.Street,
                ServiceImages = serviceURLs,
                SpecialSign = request.SpecialSign
            };

            return new MakeOfferEventResultDto
            {
                ClientOffer = clientOffer,
                TechnicianOffer = techOffer,
                ClientUserId = request.Client.UserId
            };
        }
    }
}
