
namespace Shared.DataTransferObject.NotificationDTOs
{
    public class NotificationDto
    {
        public string Title { get; set; } = default!;
        public string Body { get; set; } = default!;

        public string Action { get; set; } = default!;
        public object? ExtraPayload { get; set; }
    }
}
