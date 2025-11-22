using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Exceptions
{
    public class ServiceNotFoundException(string Service) : NotFoundException($"الحرفة '{Service}' غير موجودة")
    {
    }
}
