
using DomainLayer.Contracts;
using DomainLayer.Exceptions;
using DomainLayer.Models.IdentityModule;
using ServiceAbstraction;

namespace Service
{
    public class ClientTechnicianCommonService(IReservationRepository reservationRepository,
        IClientRepository clientRepository,
        ITechnicianRepository technicianRepository) : IClientTechnicianCommonService
    {
        public async Task<(int reservationId, string userId)> CancelReservationAsync(int reservationId, string userId, string role)
        {
            if (!await reservationRepository.IsReservationFound(reservationId))
                throw new TechnicalException();

            if (await reservationRepository.IsReservationCancelled(reservationId))
                throw new ReservationAlreadyCancelledException();

            if (!await reservationRepository.CanCancelReservation(reservationId))
                throw new TooLateToCancelReservationException();
            string targetUserId = "";
            try
            {
                var reservation = await reservationRepository.CancelReservation(reservationId, role == "Client");
                Technician? tech = null;
                Client? client = null;
                if (role == "Client")
                {

                    tech = await technicianRepository.GetByIdAsync(reservation.Offer.TechnicianId);
                    if (tech == null)
                        throw new TechnicalException();

                    targetUserId = tech.UserId;
                }
                else
                {
                    client = await clientRepository.GetByIdAsync(reservation.Offer.ServiceRequest.ClientId);
                    if (client == null)
                        throw new TechnicalException();

                    targetUserId = client.UserId;
                }
                return (reservationId, targetUserId);
            }
            catch
            {
                throw new TechnicalException();
            }
        }
    }
}
