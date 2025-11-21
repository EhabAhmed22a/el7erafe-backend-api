using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Exceptions
{
    public sealed class TechnicianPendingException() : Exception("The technician's account is still pending approval.")
    {
    }
}
