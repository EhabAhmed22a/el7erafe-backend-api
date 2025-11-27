namespace DomainLayer.Exceptions
{
    public class UnverifiedClientLogin: Exception
    {
        public string _email {  get; set; }
        public UnverifiedClientLogin(string email): base("يرجى التحقق من حسابك أولاً لتسجيل الدخول. تم إرسال رمز التحقق إلى بريدك الإلكتروني.")
        {
            _email = email;
        }
    }
}