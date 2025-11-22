namespace DomainLayer.Exceptions
{
    public class UnverifiedClientLogin() : Exception("يرجى التحقق من حسابك أولاً لتسجيل الدخول. تم إرسال رمز التحقق إلى بريدك الإلكتروني.")
    {
    }
}