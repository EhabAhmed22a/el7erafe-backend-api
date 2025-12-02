
namespace Shared.DataTransferObject.AdminDTOs.Dashboard
{
    public class BlockUnblockDTO
    {
        public bool IsBlocked {  get; set; }
        public DateTime? SuspendTo { get; set; }
        public string? SuspensionReason { get; set; }
    }
}
