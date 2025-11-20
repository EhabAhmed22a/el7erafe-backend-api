
namespace DomainLayer.Exceptions
{
    public sealed class InvalidOtpException(): Exception("رمز التحقق غير صحيح أو منتهي الصلاحية.")
    {
    }
}
