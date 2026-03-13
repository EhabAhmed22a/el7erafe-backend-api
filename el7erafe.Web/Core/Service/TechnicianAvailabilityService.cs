using DomainLayer.Contracts;
using DomainLayer.Exceptions;
using DomainLayer.Models.IdentityModule;
using DomainLayer.Models.IdentityModule.Enums;
using ServiceAbstraction;
using Shared.DataTransferObject.TechnicianSchedule;

namespace Service
{
    public class TechnicianAvailabilityService(
        ITechnicianAvailabilityRepository technicianAvailabilityRepository,
        ITechnicianRepository technicianRepository,
        ICityRepository cityRepository) : ITechnicianAvailabilityService
    {
        public async Task<List<TechnicianAvailabilityResponseDto>> CreateScheduleAsync(string technicianId, List<AvailabilityBlockDto> blocks)
        {
            var technician = await technicianRepository.GetByUserIdAsync(technicianId);
            if (technician is null) 
                throw new TechNotFoundException(technicianId);

            if (blocks == null || !blocks.Any())
                throw new ArgumentException("يجب إدخال فترة زمنية واحدة على الأقل");

            ValidateSchedule(blocks);

            await technicianAvailabilityRepository.DeleteByTechnicianIdAsync(technician.Id);

            var created = new List<TechnicianAvailability>();

            foreach (var block in blocks)
            {
                var availability = new TechnicianAvailability
                {
                    TechnicianId = technician.Id,
                    DayOfWeek = block.DayOfWeek.HasValue
                        ? (WeekDay)block.DayOfWeek.Value
                        : null,
                    FromTime = block.FromTime,
                    ToTime = block.ToTime
                };

                created.Add(await technicianAvailabilityRepository.CreateAsync(availability));
            }

            return created.Select(a => new TechnicianAvailabilityResponseDto
            {
                Id = a.Id,
                DayOfWeek = (int?)a.DayOfWeek,
                FromTime = a.FromTime,
                ToTime = a.ToTime
            }).ToList();
        }

        public async Task<List<TechnicianAvailabilityResponseDto>> GetTechnicianAvailableTimeAsync(string technicianId)
        {
            var technician = await technicianRepository.GetByUserIdAsync(technicianId);
            if (technician is null) 
                throw new TechNotFoundException(technicianId);

            var availabilities = await technicianAvailabilityRepository.GetByTechnicianIdAsync(technician.Id);
            if (availabilities is null || !availabilities.Any())
                return new List<TechnicianAvailabilityResponseDto>();

            return availabilities.Select(a => new TechnicianAvailabilityResponseDto
            {
                Id = a.Id,
                DayOfWeek = (int?)a.DayOfWeek,
                FromTime = a.FromTime,
                ToTime = a.ToTime
            }).ToList();
        }

        public async Task<List<string>> GetAvailableTechnicianByUserIdsAsync(int serviceId, int govId, DateOnly date, TimeOnly? from, TimeOnly? to)
        {
            try
            {
                var requestedDay = (WeekDay)date.DayOfWeek;

                var availableTechs = await technicianAvailabilityRepository.GetAvailableTechsForRequestAsync(serviceId, govId, requestedDay, from, to);

                return availableTechs.ToList();
            }
            catch
            {
                throw new TechnicalException();
            }
        }

        private void ValidateSchedule(List<AvailabilityBlockDto> blocks)
        {
            bool hasNull = blocks.Any(b => b.DayOfWeek == null);
            bool hasSpecific = blocks.Any(b => b.DayOfWeek != null);

            if (hasNull && hasSpecific)
                throw new Exception("لا يمكن الجمع بين التوفر لكل أيام الأسبوع والتوفر لأيام محددة");

            // Validate time format (hours only)
            foreach (var block in blocks)
            {
                if (block.FromTime.Minute != 0 || block.ToTime.Minute != 0 ||
                    block.FromTime.Second != 0 || block.ToTime.Second != 0)
                {
                    throw new Exception("يجب إدخال الوقت بالساعات فقط مثل 10:00 أو 14:00");
                }
            }

            foreach (var group in blocks.GroupBy(b => b.DayOfWeek))
            {
                var ordered = group.OrderBy(b => b.FromTime).ToList();

                for (int i = 1; i < ordered.Count; i++)
                {
                    if (ordered[i].FromTime < ordered[i - 1].ToTime)
                        throw new Exception("الفترات الزمنية المحددة متداخلة");
                }
            }
        }
    }
}
