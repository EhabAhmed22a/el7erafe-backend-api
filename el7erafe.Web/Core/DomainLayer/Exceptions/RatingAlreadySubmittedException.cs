namespace DomainLayer.Exceptions
{
    public class RatingAlreadySubmittedException() : Exception("لقد قمت بتقييم هذه الخدمة مسبقاً. لا يمكن تقييم نفس الخدمة مرتين.")
    {
    }
}
