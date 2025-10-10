
namespace DomainLayer.Exceptions
{
    public sealed class NationalIdAlreadyExists(string NationalId) : AlreadyExistException($"Phone Number '{NationalId}' is already registered.")
    {
    }
}
