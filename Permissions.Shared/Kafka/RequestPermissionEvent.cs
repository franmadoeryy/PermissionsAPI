

using Permissions.Shared.Kafka.Interfaces;

namespace Permissions.Shared.Kafka
{
    public class RequestPermissionEvent : IEventBase
    {
        public Guid Id { get; set; }
        public string Operation { get; set; }
        public int PermissionId { get; set; }
        public string EmployeeFullName { get; set; }

    }
}
