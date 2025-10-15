
namespace DomainLayer.Exceptions
{
    public sealed class NationalIdAlreadyExists(string NationalId) : AlreadyExistException($"National Id '{NationalId}' is already registered.")
    {
    }
}
