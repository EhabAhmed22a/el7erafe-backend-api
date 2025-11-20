
namespace DomainLayer.Exceptions
{
    public class EmailAlreadyExists(string email): AlreadyExistException($"البريد الإلكتروني مسجل بالفعل.")
    {
    }
}
