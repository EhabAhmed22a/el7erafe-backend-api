using System.ComponentModel.DataAnnotations;

namespace Shared.DataTransferObject.ClientIdentityDTOs
{
    public class ClientLoginDTO
    {
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        public string PhoneNumber { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
