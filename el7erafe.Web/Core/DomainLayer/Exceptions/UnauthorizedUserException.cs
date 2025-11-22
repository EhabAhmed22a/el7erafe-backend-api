
namespace DomainLayer.Exceptions
{
    public sealed class UnauthorizedUserException(string Message = "رقم الهاتف أو كلمة المرور غير صحيحة.") : UnauthorizedException(Message) 
    {
    }
}
