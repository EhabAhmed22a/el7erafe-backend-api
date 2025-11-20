namespace DomainLayer.Exceptions
{
    public sealed class OtpAlreadySent() : Exception("تم إرسال رمز التحقق بالفعل. يرجى الانتظار لمدة دقيقة واحدة قبل إعادة المحاولة")
    {
    }
}