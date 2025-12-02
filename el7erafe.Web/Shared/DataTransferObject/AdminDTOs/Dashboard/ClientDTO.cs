
namespace Shared.DataTransferObject.AdminDTOs.Dashboard
{
    public class ClientDTO
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public bool? EmailConfirmed { get; set; }
        public string? PhoneNumber { get; set; }
        public bool  IsBlocked { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
