
namespace Shared.DataTransferObject.AdminDTOs.Dashboard
{
    public class RejectTechDTO
    {
        public string id { get; set; } = default!;
        public string rejectionReason { get; set; } = default!;
        public bool is_front_rejected { get; set; }
        public bool is_back_rejected { get; set; }
        public bool is_criminal_rejected { get; set; }
    }
}
