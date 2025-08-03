using Permissions.Shared.Kafka.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Permissions.Shared.Kafka
{
    public class ModifyPermissionEvent : IEventBase
    {
        public Guid Id { get; set; }
        public string Operation { get; set; }
        public int PermissionId { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeLastName { get; set; }
        public int PermissionTypeId { get; set; }
        public DateTime PermissionDate { get; set; }
    }
}
