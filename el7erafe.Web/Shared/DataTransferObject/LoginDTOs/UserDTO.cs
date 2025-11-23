
namespace Shared.DataTransferObject.LoginDTOs
{
    public class UserDTO
    {
        public string userName { get; set; } = null!;
        public string userId { get; set; } = null!;
        public char type { get; set; }
        public string token { get; set; } = null!;
    }
}
