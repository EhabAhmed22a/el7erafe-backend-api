
namespace Shared.DataTransferObject
{
    public class UserDTO
    {
        public string token { get; set; } = default!;
        public string userId { get; set; } = default!;
        public string userName { get; set; } = default!;
        public char type { get; set; } = default!; 

    }
}