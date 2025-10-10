
namespace DomainLayer.Exceptions
{
    public class EmailAlreadyExists(string email): AlreadyExistException($"Email '{email}' is already registered")
    {
    }
}
