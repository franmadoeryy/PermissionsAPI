using Permissions.Shared.Kafka.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Permissions.Shared.Kafka
{
    public class GetPermissionsEvent : IEventBase
    {
        public Guid Id { get; set; }
        public string Operation { get; set; }
        public List<int> PermissionIds { get; set; }
    }
}
