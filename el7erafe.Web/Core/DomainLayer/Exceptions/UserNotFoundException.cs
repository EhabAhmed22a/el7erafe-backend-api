using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Exceptions
{
    public sealed class TechNotFoundException(string phone) : NotFoundException($"Technician With Phone Number {phone} is Not Found")
    {
    }
}
