
namespace DomainLayer.Exceptions
{
    public class UnverifiedTechnicianLogin(string email) : UnverifiedException(email)
    {
    }
}
