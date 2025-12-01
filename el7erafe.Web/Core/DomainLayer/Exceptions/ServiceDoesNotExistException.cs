
namespace DomainLayer.Exceptions
{
    public class ServiceDoesNotExistException() : NotFoundException("الخدمة غير موجودة")
    {
    }
}
