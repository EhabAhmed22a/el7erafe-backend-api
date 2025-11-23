
namespace DomainLayer.Exceptions
{
    public class PendingTechnicianRequest: Exception
    {
        public string _tempToken {  set; get; }
        public PendingTechnicianRequest(string tempToken): base("يرجى انتظار موافقة مسؤول على ملفاتك الشخصية للمتابعة في تسجيل الدخول.")
        {
            _tempToken = tempToken;
        }
    }
}
