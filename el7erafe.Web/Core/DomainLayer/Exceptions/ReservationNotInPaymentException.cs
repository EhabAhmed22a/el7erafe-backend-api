

namespace DomainLayer.Exceptions
{
    public class ReservationNotInPaymentException(): Exception("لا يمكن الدفع الآن. يرجى الانتظار حتى يكمل الفني العمل.")
    {
    }
}
