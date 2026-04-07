

namespace DomainLayer.Exceptions
{
    public class TooLateToCancelReservationException(): Exception("(لا يمكن إلغاء الحجز لتجاوز الوقت المسموح (يجب الإلغاء قبل الموعد بساعة على الأقل).")
    {
    }
}
