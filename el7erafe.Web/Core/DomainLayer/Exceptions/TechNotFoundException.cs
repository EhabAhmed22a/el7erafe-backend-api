
namespace DomainLayer.Exceptions
{
    public sealed class TechNotFoundException(string TechId) : NotFoundException($"Technician With Id {TechId} is Not Found")
    {
    }
}
