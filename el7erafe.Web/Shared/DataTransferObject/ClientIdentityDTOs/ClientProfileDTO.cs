
namespace Shared.DataTransferObject.ClientIdentityDTOs
{
    public class ClientProfileDTO
    {
        public string Name { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string? ImageURL {  get; set; }
    }
}
