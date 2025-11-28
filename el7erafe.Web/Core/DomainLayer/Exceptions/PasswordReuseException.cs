
namespace DomainLayer.Exceptions
{
    public class PasswordReuseException(): Exception("كلمة المرور الجديدة لا يمكن أن تكون مشابهة لكلمة المرور الحالية")
    {
    }
}
