
namespace Shared.DataTransferObject.ClientDTOs
{
    public class GetAvailableTechniciansRequest
    {
        public string ServiceName { get; set; } = default!;

        public string CityName { get; set; } = default!;

        public bool Sorted { get; set; } 
    }

}
