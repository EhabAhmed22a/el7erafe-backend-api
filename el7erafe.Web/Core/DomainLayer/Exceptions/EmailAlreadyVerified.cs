
namespace DomainLayer.Exceptions
{
    public sealed class EmailAlreadyVerified(string message): Exception(message)
    {
    }
}
