
using System.Text.Json.Serialization;

namespace Shared.DataTransferObject.OffersDTOs
{
    public class OfferResultDto
    {
        public int OfferId { get; set; }

        public int RequestId { get; set; }

        public string? TechName { get; set; } = default!;

        public string? TechImage { get; set; } = default!;

        public string? ServiceType { get; set; } = default!;

        public decimal? Fees { get; set; }

        public TimeOnly? FromTime { get; set; }

        public TimeOnly? ToTime { get; set; }

        public DateOnly? Day { get; set; }

        public int? NumberOfSuccessJobs { get; set; }

        public decimal? Rate { get; set; }

        public string? Comments { get; set; } = default!;

        public int? NumberOfDays { get; set; }

        [JsonIgnore]
        public int ClientId { get; set; }
    }
}
