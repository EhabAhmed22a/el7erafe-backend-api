
using Shared.DataTransferObject.TechnicianSchedule;

namespace ServiceAbstraction
{
    public interface ITechnicianAvailabilityService
    {
        Task<List<TechnicianAvailabilityResponseDto>> GetTechnicianAvailableTimeAsync(string technicianId);

        Task<List<TechnicianAvailabilityResponseDto>> CreateScheduleAsync(string technicianId,List<AvailabilityBlockDto> blocks);

        Task<TechnicianAvailabilityResponseDto> UpdateAsync(string technicianId,UpdateTechnicianAvailabilityDto dto);

        Task<int> DeleteTechnicianAvailableTimeAsync(string technicianId, int id);
    }
}
