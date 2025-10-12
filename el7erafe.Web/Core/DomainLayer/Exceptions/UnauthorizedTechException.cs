
namespace DomainLayer.Exceptions
{
    public sealed class UnauthorizedTechException(string Message = "Invalid Phone Number Or Password") : UnauthorizedException(Message) 
    {
    }
}
