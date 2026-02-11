
using Microsoft.AspNetCore.Http;

namespace Shared.DataTransferObject.ServiceRequestDTOs
{
    public class ServiceReqDTO
    {
        public int Id { get; set; }
        public string Description { get; set; } = default!;
        public List<IFormFile>? Images { get; set; }
        public string? ServiceName { get; set; }
        public string? City { get; set; }
        public string? Governate { get; set; }
        public string Street { get; set; } = default!;
        public string? SpecialSign { get; set; }
        public DateOnly ServiceDate { get; set; } = default!;
        public TimeOnly? AvailableFrom { get; set; } 
        public TimeOnly? AvailableTo { get; set; } 
        public bool AllDayAvailability {  get; set; }
        public string? ImageURL { get; set; }
    }
}
