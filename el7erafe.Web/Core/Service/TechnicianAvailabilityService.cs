using DomainLayer.Contracts;
using DomainLayer.Exceptions;
using DomainLayer.Models.IdentityModule;
using DomainLayer.Models.IdentityModule.Enums;
using Service.Helpers;
using ServiceAbstraction;
using Shared.DataTransferObject.TechnicianSchedule;

namespace Service
{
    public class TechnicianAvailabilityService(
        ITechnicianAvailabilityRepository technicianAvailabilityRepository,
        ITechnicianRepository technicianRepository) : ITechnicianAvailabilityService
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

        public async Task<List<string>> GetAvailableTechnicianByUserIdsAsync(int serviceId, int govId, DateOnly date, TimeOnly? from, TimeOnly? to, TimeOnly? minTime)
        {
            try
            {
                var requestedDay = HelperClass.MapToWeekDay(date.DayOfWeek);

                var availableTechs = await technicianAvailabilityRepository.GetAvailableTechsForRequestAsync(serviceId, govId, requestedDay, from, to, minTime);

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

                if (block.FromTime.Minute != 0 || block.FromTime.Second != 0)
                {
                    throw new Exception("يجب أن يكون وقت البداية على رأس الساعة تماماً (مثال: 14:00 أو 10:00)");
                }

                bool isToTimeOnHour = block.ToTime.Minute == 0 && block.ToTime.Second == 0;
                bool isEndOfDay = block.ToTime.Hour == 23 && block.ToTime.Minute == 59;

                if (!isToTimeOnHour && !isEndOfDay)
                {
                    throw new Exception("يجب إدخال الوقت بالساعات فقط (مثل 10:00 أو 14:00)، باستثناء نهاية اليوم 23:59");
                }
            }

            foreach (var group in blocks.GroupBy(b => b.DayOfWeek))
            {
                var ordered = group.OrderBy(b => b.FromTime).ToList();

                for (int i = 1; i < ordered.Count; i++)
                {
                    if (ordered[i].FromTime < ordered[i - 1].ToTime)
                        throw new ArgumentException("الفترات الزمنية المحددة متداخلة");
                }
            }
        }
    }
}
