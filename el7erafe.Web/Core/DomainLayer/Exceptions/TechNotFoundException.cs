
namespace DomainLayer.Exceptions
{
    public sealed class TechNotFoundException(string phone) : NotFoundException($"Technician With Phone Number {phone} is Not Found")
    {
    }
}
