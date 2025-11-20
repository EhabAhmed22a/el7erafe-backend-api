
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DomainLayer.Exceptions
{
    public sealed class EmailAlreadyVerified(string message): Exception(message)
    {
    }
}
