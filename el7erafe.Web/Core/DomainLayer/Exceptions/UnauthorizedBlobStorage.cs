
namespace DomainLayer.Exceptions
{
    public sealed class UnauthorizedBlobStorage() : UnauthorizedException("This request is not authorized to perform this operation")
    {
    }
}
