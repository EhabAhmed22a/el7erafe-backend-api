using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Exceptions
{
    public sealed class UnauthorizedTechException(string Message = "Invalid Phone Number Or Password") : UnauthorizedException(Message) 
    {
    }
}
