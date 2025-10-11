
namespace DomainLayer.Exceptions
{
    public sealed class InvalidOtpException(): Exception("Invalid or expired OTP.")
    {
    }
}
