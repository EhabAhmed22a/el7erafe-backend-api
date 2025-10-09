using System.ComponentModel.DataAnnotations;

namespace Shared.DataTransferObject.TechnicianIdentityDTOs
{
    public class TechLoginDTO
    {
        [Phone]
        public string PhoneNumber { get; set; } = default!;
        public string Password { get; set; } = default!; 
    }
}
