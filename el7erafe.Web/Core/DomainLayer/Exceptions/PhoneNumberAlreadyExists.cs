
namespace DomainLayer.Exceptions
{
    public sealed class PhoneNumberAlreadyExists(string phoneNumber): Exception($"Phone Number '{phoneNumber}' is already registered.")
    {
        public string PhoneNumber { get; set; } = phoneNumber;
    }
}
