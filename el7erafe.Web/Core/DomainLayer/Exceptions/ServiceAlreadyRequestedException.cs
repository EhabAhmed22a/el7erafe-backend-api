
namespace DomainLayer.Exceptions
{
    public class ServiceAlreadyRequestedException() : Exception("لقد قمت بالفعل بتقديم طلب لهذه الخدمة مسبقاً، يرجى انتظار عروض الفنيين للرد على طلبك")
    {
    }
}