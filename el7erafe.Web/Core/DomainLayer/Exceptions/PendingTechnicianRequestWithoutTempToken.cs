
namespace DomainLayer.Exceptions
{
    public sealed class PendingTechnicianRequestWithoutTempToken() : Exception("يرجى انتظار موافقة مسؤول على ملفاتك الشخصية للمتابعة في تسجيل الدخول.")
    {
    }
}
