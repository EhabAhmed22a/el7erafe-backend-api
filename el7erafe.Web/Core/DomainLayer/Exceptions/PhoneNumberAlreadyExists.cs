
namespace DomainLayer.Exceptions
{
    public sealed class PhoneNumberAlreadyExists(string phoneNumber): AlreadyExistException($"رقم الهاتف '{phoneNumber}' مسجل بالفعل.")
    {
    }
}
