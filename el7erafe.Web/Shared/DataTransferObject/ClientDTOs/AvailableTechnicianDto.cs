
namespace Shared.DataTransferObject.ClientDTOs
{
    public class AvailableTechnicianDto
    {
        public string Id { get; set; } = default!;

        public string Name { get; set; } = default!;

        public string ServiceName { get; set; } = default!;

        public decimal Rating { get; set; }

        public string City { get; set; } = default!;

        public string About { get; set; } = default!;

        public string ProfilePicture { get; set; } = default!;

        public List<string> PortfolioImages { get; set; } = new();
    }

}
