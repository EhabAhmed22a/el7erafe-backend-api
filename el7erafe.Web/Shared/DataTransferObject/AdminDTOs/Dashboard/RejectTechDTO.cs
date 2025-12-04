
using System.ComponentModel.DataAnnotations;

namespace Shared.DataTransferObject.AdminDTOs.Dashboard
{
    public class RejectTechDTO
    {
        [Required(ErrorMessage = "يجب إدخال id المستخدم")]
        public string id { get; set; } = default!;
        [Required(ErrorMessage = "سبب الرفض مطلوب")]
        public string rejectionReason { get; set; } = default!;
        public bool is_front_rejected { get; set; }
        public bool is_back_rejected { get; set; }
        public bool is_criminal_rejected { get; set; }
    }
}
