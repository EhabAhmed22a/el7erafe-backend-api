
using System.ComponentModel.DataAnnotations;

namespace Shared.DataTransferObject.ServiceRequestDTOs
{
    public class CancelReqDTO
    {
        [Required]
        public int requestId { get; set; }
    }
}
