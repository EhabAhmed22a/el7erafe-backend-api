
using System.ComponentModel.DataAnnotations;

namespace Shared.DataTransferObject.ServiceRequestDTOs
{
    public class ReqIdDTO
    {
        [Required]
        public int requestId { get; set; }
    }
}
