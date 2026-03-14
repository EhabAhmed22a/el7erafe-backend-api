
namespace DomainLayer.Exceptions
{
    public class RequestAlreadyCanceledException(string message = "لقد قمت بالغاء هذا الطلب مسبقاً") : Exception(message)
    {
    }
}