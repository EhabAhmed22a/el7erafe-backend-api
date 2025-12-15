
namespace DomainLayer.Exceptions
{
    public abstract class UnverifiedException(string email) : Exception("يرجى التحقق من حسابك أولاً لتسجيل الدخول. تم إرسال رمز التحقق إلى بريدك الإلكتروني")
    {
        public string _email { get; set; } = email;
    }
}
