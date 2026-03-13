
namespace Shared.DataTransferObject.ServiceRequestDTOs
{
    public class ServiceRequestDTO
    {
        public int requestId { get; set; }
        public bool isQuickReserve { get; set; }
        public int? numberOfOffers { get; set; }
        public string? clientTimeInterval { get; set; }
        public string? techTimeInterval { get; set; }
        public string serviceType { get; set; } = default!;
        public string? techName { get; set; }
        public string? techImage { get; set; }
        public DateOnly day { get; set; }
        public decimal? fees { get; set; }
        public int? offerId { get; set; }
        public int? numberOfDays { get; set; }
    }
}
