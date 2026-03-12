
using Shared.DataTransferObject.TechnicianSchedule;

namespace ServiceAbstraction
{
    public interface ITechnicianAvailabilityService
    {
        Task<List<TechnicianAvailabilityResponseDto>> GetTechnicianAvailableTimeAsync(string technicianId);

        Task<List<TechnicianAvailabilityResponseDto>> CreateScheduleAsync(string technicianId,List<AvailabilityBlockDto> blocks);
    }
}
