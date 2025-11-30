
using System.ComponentModel.DataAnnotations;

namespace Shared.DataTransferObject.AdminDTOs.LoginDTO
{
    public class AdminLoginDTO
    {
        [Required(ErrorMessage = "اسم المستخدم مطلوب")]
        public string Username { get; set; }
        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        public string Password { get; set; }
    }
}
