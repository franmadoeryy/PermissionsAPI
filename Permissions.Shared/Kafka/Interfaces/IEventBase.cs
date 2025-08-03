using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Permissions.Shared.Kafka.Interfaces
{
    public interface IEventBase
    {
        public Guid Id { get; set; }
        public string Operation { get; set; }
    }
}
