
namespace DomainLayer.Exceptions
{
    public class ServiceAlreadyRegisteredException(): Exception("هذه الخدمة مسجلة بالفعل. لا يمكن تكرار تسجيل الخدمة")
    {
    }
}
