
namespace DomainLayer.Exceptions
{
    public sealed class UserNotFoundException(): NotFoundException("User Not Found")
    {
    }
}
