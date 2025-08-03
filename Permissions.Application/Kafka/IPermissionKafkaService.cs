using Permissions.Shared.Kafka.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Permissions.Application.Kafka
{
    public interface IPermissionKafkaService
    {
        Task PublishPermissionEventAsync(IEventBase evt, CancellationToken cancellationToken = default);

    }
}
