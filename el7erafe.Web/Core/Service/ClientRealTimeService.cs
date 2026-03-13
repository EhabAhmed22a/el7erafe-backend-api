
using System.Globalization;
using DomainLayer.Contracts;
using DomainLayer.Contracts.ChatModule;
using DomainLayer.Exceptions;
using DomainLayer.Models.ChatModule;
using Service.Helpers;
using ServiceAbstraction;
using Shared.DataTransferObject.ServiceRequestDTOs;

namespace Service
{
    public class ClientRealTimeService(IUserConnectionRepository userConnectionRepository,
        IClientRepository clientRepository,
        IServiceRequestRepository serviceRequestRepository,
        IBlobStorageRepository blobStorageRepository) : IClientRealTimeService
    {
        public async Task<UserConnection> AddUserConnectionAsync(string userId, string connectionId)
        {
            return await userConnectionRepository.AddConnectionAsync(userId, connectionId);
        }

        public async Task RemoveConnectionAsync(string userId)
        {
            try
            {
                await userConnectionRepository.RemoveConnectionAsync(userId);
            }
            catch
            {
                throw new TechnicalException();
            }
        }

        public async Task<List<ServiceRequestDTO>> GetServiceRequestsAsync(string userId)
        {
            var user = await clientRepository.GetByUserIdAsync(userId);
            if (user is null)
                throw new UserNotFoundException("المستخدم غير موجود");

            var notReservedRequests = await serviceRequestRepository.GetNotResServiceRequestsByClientAsync(user.Id);

            var mappingTasks = notReservedRequests.Select(async sr =>
            {
                string? imageUrl = null;
                if (!string.IsNullOrWhiteSpace(sr.Technician?.ProfilePictureURL))
                {
                    imageUrl = await blobStorageRepository.GetBlobUrlWithSasTokenAsync("technician-documents", sr.Technician.ProfilePictureURL);
                }
                return new ServiceRequestDTO
                {
                    requestId = sr.Id,
                    isQuickReserve = sr.TechnicianId is null,
                    day = sr.ServiceDate,
                    serviceType = sr.Service?.NameAr ?? "غير معروف",

                    numberOfOffers = sr.Offers.Count,
                    clientTimeInterval = HelperClass.FormatArabicTimeInterval(sr.AvailableFrom, sr.AvailableTo),

                    techName = sr.Technician?.Name,
                    techImage = imageUrl,

                    offerId = sr.Offers?.FirstOrDefault()?.Id,
                    fees = sr.Offers?.FirstOrDefault()?.Fees,

                    techTimeInterval = HelperClass.FormatArabicTimeInterval(
                        sr.Offers?.FirstOrDefault()?.WorkFrom,
                        sr.Offers?.FirstOrDefault()?.WorkTo
                    ),

                    numberOfDays = sr.Offers?.FirstOrDefault()?.NumberOfDays ?? 1
                };
            });

            return (await Task.WhenAll(mappingTasks)).ToList();
        }
    }
}
