
namespace DomainLayer.Exceptions
{
    public class ResetTokenExpiredException(): Exception("انتهت صلاحية الرابط، يرجى العودة إلى صفحة نسيان كلمة المرور وطلب رابط جديد")
    {
    }
}
