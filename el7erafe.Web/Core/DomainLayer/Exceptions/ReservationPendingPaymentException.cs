
namespace DomainLayer.Exceptions
{
    public class ReservationPendingPaymentException(): Exception("لديك حجز بانتظار الدفع. يرجى إتمام عملية الدفع أولاً لتتمكن من طلب خدمات جديدة.")
    {
    }
}
