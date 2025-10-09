
using System.ComponentModel.DataAnnotations;

namespace Shared.DataTransferObject.ClientIdentityDTOs
{
    public class ClientDTO
    {
        public string Name { get; set; } = null!;
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        public int PhoneNumber { get; set; }
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string token { get; set; } = null!;
        public string refreshToken { get; set; } = null!;
    }
}
