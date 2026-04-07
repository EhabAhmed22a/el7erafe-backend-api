
namespace DomainLayer.Exceptions
{
    public class PendingServiceAlreadyRequestedException() : Exception("لقد قمت بالفعل بتقديم طلب لهذه الخدمة مسبقاً، يرجى انتظار عروض الفنيين للرد على طلبك")
    {
    }
}