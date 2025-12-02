
namespace Shared.DataTransferObject.AdminDTOs.Dashboard
{
    public class TechnicianListDTO
    {
        public int Count { get; set; }
        public IEnumerable<TechnicianDTO> Data { get; set; } = new List<TechnicianDTO>();
    }
}
