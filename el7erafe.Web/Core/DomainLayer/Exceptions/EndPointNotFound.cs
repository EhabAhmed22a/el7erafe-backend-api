
namespace DomainLayer.Exceptions
{
    public sealed class EndPointNotFound(string Message) : NotFoundException(Message)
    {
    }
}
