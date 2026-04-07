

namespace DomainLayer.Exceptions
{
    public class ReservationAlreadyCancelledException(): Exception("هذا الحجز ملغي بالفعل ولا يمكن اتخاذ أي إجراء عليه.")
    {
    }
}
