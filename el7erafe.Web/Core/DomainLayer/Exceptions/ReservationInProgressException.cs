
namespace DomainLayer.Exceptions
{
    public class ReservationInProgressException(): Exception("الفني يقوم بتنفيذ هذه الخدمة لك حالياً. لا يمكنك طلب نفس الخدمة مرة أخرى حتى يتم الانتهاء منها.")
    {
    }
}
