
using System.Text.Json.Serialization;

namespace Shared.DataTransferObject.OffersDTOs
{
    public class OfferResultDto
    {
        public int OfferId { get; set; }

        public int RequestId { get; set; }

        public string? TechName { get; set; }

        public string? TechImage { get; set; }

        public string? ServiceType { get; set; }

        public decimal? Fees { get; set; }

        public string? TechTimeInterval { get; set; }

        public DateOnly? Day { get; set; }

        public int? NumberOfSuccessJobs { get; set; }

        public decimal? Rate { get; set; }

        public string? Comments { get; set; }

        public int? NumberOfDays { get; set; }

        [JsonIgnore]
        public int ClientId { get; set; }
    }
}
