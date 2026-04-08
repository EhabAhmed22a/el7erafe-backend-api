namespace DomainLayer.Exceptions
{
    public class ReservationNotCompletedException() : Exception("لا يمكن تقييم الفني إلا بعد اكتمال الخدمة ودفع التكاليف.")
    {
    }
}