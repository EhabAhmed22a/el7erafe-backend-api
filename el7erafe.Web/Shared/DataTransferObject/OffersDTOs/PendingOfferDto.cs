
namespace Shared.DataTransferObject.OffersDTOs
{
    public class PendingOfferDto
    {
        public int OfferId { get; set; }

        public string? Description { get; set; }

        public string? ClientName { get; set; }

        public string? ClientImage { get; set; }

        public decimal Fees { get; set; }

        public string? ServiceType { get; set; }

        public string? TechTimeInterval { get; set; }

        public DateOnly Day { get; set; }

        public DateOnly? EndDay { get; set; }

        public List<string>? ServiceImages { get; set; }

        public string? Governorate { get; set; }

        public string? City { get; set; }

        public string? Street { get; set; }

        public string? SpecialSign { get; set; }
    }
}
