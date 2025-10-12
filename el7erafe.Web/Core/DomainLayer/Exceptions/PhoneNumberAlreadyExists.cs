
namespace DomainLayer.Exceptions
{
    public sealed class PhoneNumberAlreadyExists(string phoneNumber): AlreadyExistException($"Phone Number '{phoneNumber}' is already registered.")
    {
    }
}
