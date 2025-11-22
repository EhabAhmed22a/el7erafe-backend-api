
namespace DomainLayer.Exceptions
{
    public sealed class BadRequestException(List<string> errors) : Exception("البيانات المدخلة غير صالحة")
    {
        public List<string> Errors { get; } = errors;
    }
}
