
namespace Shared.DataTransferObject.AdminDTOs.Dashboard
{
    public class ServiceListDTO
    {
        public int Count { get; set; }
        public IEnumerable<ServiceDTO> Services { get; set; } = new List<ServiceDTO>();
    }
}
