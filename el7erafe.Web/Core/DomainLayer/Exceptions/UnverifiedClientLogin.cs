namespace DomainLayer.Exceptions
{
    public class UnverifiedClientLogin(string email) : UnverifiedException(email)
    {
    }
}