
namespace DomainLayer.Exceptions
{
    public class InvalidRatingValueException(): Exception("التقييم يجب أن يكون رقماً صحيحاً بين 1 و 5.")
    {
    }
}
