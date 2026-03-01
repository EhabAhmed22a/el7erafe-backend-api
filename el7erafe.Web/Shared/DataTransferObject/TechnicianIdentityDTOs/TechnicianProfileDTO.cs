
namespace Shared.DataTransferObject.TechnicianIdentityDTOs
{
    public class TechnicianProfileDTO
    {
        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Phone {  get; set; } = default!;
        public string? ProfileImage { get; set; } = default!;
        public string? AboutMe { get; set; }
        public List<string> PortifolioImages { get; set; } = new List<string>();
    }
}
