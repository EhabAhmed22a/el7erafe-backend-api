
namespace DomainLayer.Exceptions
{
    public sealed class UnauthorizedLogoutException(string message) : UnauthorizedException(message)
    {
    }
}
