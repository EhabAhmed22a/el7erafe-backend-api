

namespace DomainLayer.Exceptions
{
    public class RequestAlreadyDeclinedException(string message = "لقد قمت برفض هذا الطلب مسبقاً") : Exception (message)
    {
    }
}