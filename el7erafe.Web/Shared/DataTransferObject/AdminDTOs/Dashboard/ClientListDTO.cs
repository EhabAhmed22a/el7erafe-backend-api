using Shared.DataTransferObject;

namespace Shared.DataTransferObject.AdminDTOs.Dashboard
{
    public class ClientListDTO
    {
        public int Count { get; set; }
        public IEnumerable<ClientDTO> Data { get; set; } = new List<ClientDTO>();
    }
}
