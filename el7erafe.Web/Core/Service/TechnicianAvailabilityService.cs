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
        ITechnicianRepository technicianRepository) : ITechnicianAvailabilityService
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

        public async Task<int> DeleteTechnicianAvailableTimeAsync(string technicianId, int id)
        {
            var technician = await technicianRepository.GetByUserIdAsync(technicianId);

            if (technician is null)
                throw new TechNotFoundException(technicianId);

            var availability = await technicianAvailabilityRepository.GetByIdAsync(id);

            if (availability is null)
                throw new Exception("Availability not found");

            if (availability.TechnicianId != technician.Id)
                throw new UnauthorizedAccessException("You cannot delete this availability");

            return await technicianAvailabilityRepository.DeleteAsync(id);
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

        public async Task<TechnicianAvailabilityResponseDto> UpdateAsync(string technicianId, UpdateTechnicianAvailabilityDto dto)
        {
            var technician = await technicianRepository.GetByUserIdAsync(technicianId);

            if (technician is null)
                throw new TechNotFoundException(technicianId);

            var availability = await technicianAvailabilityRepository.GetByIdAsync(dto.Id);

            if (availability is null)
                throw new Exception("Availability not found");

            if (availability.TechnicianId != technician.Id)
                throw new UnauthorizedAccessException("You cannot update this availability");

            var existingBlocks = await technicianAvailabilityRepository.GetByTechnicianIdAsync(technician.Id);

            var blocksForValidation = existingBlocks
                .Where(a => a.Id != dto.Id)
                .Select(a => new AvailabilityBlockDto
                {
                    DayOfWeek = (int?)a.DayOfWeek,
                    FromTime = a.FromTime,
                    ToTime = a.ToTime
                })
                .ToList();

            blocksForValidation.Add(new AvailabilityBlockDto
            {
                DayOfWeek = dto.DayOfWeek,
                FromTime = dto.FromTime,
                ToTime = dto.ToTime
            });

            ValidateSchedule(blocksForValidation);

            availability.DayOfWeek = dto.DayOfWeek.HasValue ? (WeekDay)dto.DayOfWeek.Value : null;
            availability.FromTime = dto.FromTime;
            availability.ToTime = dto.ToTime;

            await technicianAvailabilityRepository.UpdateAsync(availability);

            return new TechnicianAvailabilityResponseDto
            {
                Id = availability.Id,
                DayOfWeek = (int?)availability.DayOfWeek,
                FromTime = availability.FromTime,
                ToTime = availability.ToTime
            };
        }

        private void ValidateSchedule(List<AvailabilityBlockDto> blocks)
        {
            bool hasNull = blocks.Any(b => b.DayOfWeek == null);
            bool hasSpecific = blocks.Any(b => b.DayOfWeek != null);

            if (hasNull && hasSpecific)
                throw new Exception("لا يمكن الجمع بين التوفر لكل أيام الأسبوع والتوفر لأيام محددة");

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
