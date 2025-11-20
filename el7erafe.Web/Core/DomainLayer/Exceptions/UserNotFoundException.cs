
namespace DomainLayer.Exceptions
{
    public sealed class UserNotFoundException(string message): NotFoundException(message)
    {
    }
}
