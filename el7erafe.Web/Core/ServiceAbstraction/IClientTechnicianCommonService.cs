

namespace ServiceAbstraction
{
    public interface IClientTechnicianCommonService
    {
        Task<(int reservationId, string userId)> CancelReservationAsync(int reservationId, string userId, string role);
    }
}
