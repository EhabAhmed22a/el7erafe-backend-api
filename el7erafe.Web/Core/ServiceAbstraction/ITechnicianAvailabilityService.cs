
using Shared.DataTransferObject.TechnicianSchedule;

namespace ServiceAbstraction
{
    public interface ITechnicianAvailabilityService
    {
        Task<List<TechnicianAvailabilityResponseDto>> GetTechnicianAvailableTimeAsync(string technicianId);

        Task<List<TechnicianAvailabilityResponseDto>> CreateScheduleAsync(string technicianId,List<AvailabilityBlockDto> blocks);
        Task<List<string>> GetAvailableTechnicianByUserIdsAsync(int serviceId, int govId, DateOnly date, TimeOnly? from, TimeOnly? to, TimeOnly? minTime);
    }
}
