
namespace DomainLayer.Exceptions
{
    public sealed class TechNotFoundException(string TechId) : NotFoundException($"الحرفي بالرقم '{TechId}' غير موجود")
    {
    }
}
