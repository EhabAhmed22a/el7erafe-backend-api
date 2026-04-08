
namespace DomainLayer.Exceptions
{
    public class ReservationAlreadyPaidException(): Exception("لقد تم دفع قيمة هذا الحجز وإكماله مسبقاً.")
    {
    }
}
