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
            {
                await technicianAvailabilityRepository.DeleteByTechnicianIdAsync(technician.Id);
                return [];
            }

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
                DayOfWeek = (int?)a.DayOfWeek,
                FromTime = a.FromTime,
                ToTime = a.ToTime
            }).ToList();
        }

        public async Task<List<TechnicianAvailabilityResponseDto>> GetTechnicianAvailableTimeAsync(string technicianId)
        {
            // if to time retusn as 23:59 return it as 12 am
            var technician = await technicianRepository.GetByUserIdAsync(technicianId);
            if (technician is null) 
                throw new TechNotFoundException(technicianId);

            var availabilities = await technicianAvailabilityRepository.GetByTechnicianIdAsync(technician.Id);
            if (availabilities is null || !availabilities.Any())
                return new List<TechnicianAvailabilityResponseDto>();

            return availabilities.Select(a => new TechnicianAvailabilityResponseDto
            {
                DayOfWeek = (int?)a.DayOfWeek,
                FromTime = a.FromTime,
                ToTime = a.ToTime == new TimeOnly(23, 59) ? new TimeOnly(0, 0) : a.ToTime
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

            foreach (var block in blocks)
            {
                if (block.FromTime >= block.ToTime)
                    throw new Exception("وقت البداية يجب أن يكون قبل وقت النهاية");

                // 1. FromTime MUST be exactly on the hour (Minute == 0)
                if (block.FromTime.Minute != 0 || block.FromTime.Second != 0)
                {
                    throw new Exception("يجب أن يكون وقت البداية على رأس الساعة تماماً (مثال: 14:00 أو 10:00)");
                }

                // 2. ToTime MUST NOT be exactly on the hour (Minute != 0)
                // This accepts 14:30, 14:45, 14:59, and 23:59 perfectly!
                if (block.ToTime.Minute == 0)
                {
                    throw new Exception("يجب ألا يكون وقت النهاية على رأس الساعة (أدخل الدقائق مثل 14:30 أو 14:59)");
                }
            }

            foreach (var group in blocks.GroupBy(b => b.DayOfWeek))
            {
                var ordered = group.OrderBy(b => b.FromTime).ToList();

                // The overlap check handles any minutes perfectly now
                for (int i = 1; i < ordered.Count; i++)
                {
                    if (ordered[i].FromTime <= ordered[i - 1].ToTime)
                        throw new ArgumentException("الفترات الزمنية المحددة متداخلة");
                }
            }
        }
    }
}
