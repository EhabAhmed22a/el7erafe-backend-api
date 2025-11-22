
namespace Shared.DataTransferObject.LogoutDTOs
{
    public class LogoutResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = default!;
        public DateTime LogoutTime { get; set; }
    }
}
