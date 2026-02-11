
namespace DomainLayer.Exceptions
{
    public class ServiceRequestTimeConflictException()
        : Exception("لديك طلب خدمة آخر يتعارض مع هذا الوقت، يرجى اختيار وقت مختلف")
    {

    }
}
