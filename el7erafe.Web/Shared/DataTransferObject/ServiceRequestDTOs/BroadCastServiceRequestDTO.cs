
using System.Text.Json.Serialization;

namespace Shared.DataTransferObject.ServiceRequestDTOs
{
    public class BroadCastServiceRequestDTO
    {
        public int requestId {  get; set; }
        public string? clientName { get; set; }
        public string? clientImage { get; set; }
        public DateOnly? day { get; set; } = default!;
        public string? clientTimeInterval { get; set; }
        public string? serviceType { get; set; }
        public string? description { get; set; }
        public List<string> serviceImages { get; set; } = new List<string>();
        public string? governorate { get; set; }
        public string? city { get; set; }
        public string? street { get; set; }
        public string? specialSign { get; set; }

        [JsonIgnore]
        public int ServiceId { get; set; }
        [JsonIgnore]
        public TimeOnly? From { get; set; }
        [JsonIgnore]
        public TimeOnly? To { get; set; }

        [JsonIgnore]
        public int GovernorateId { get; set; }
    }
}
