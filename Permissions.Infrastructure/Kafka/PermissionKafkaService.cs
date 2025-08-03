using Confluent.Kafka;
using Permissions.Application.Kafka;
using Permissions.Shared.Kafka.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Permissions.Infrastructure.Kafka
{
    public class PermissionKafkaService : IPermissionKafkaService
    {
        private readonly IProducer<Null, string> _producer;
        private const string Topic = "permissions-operations";

        public PermissionKafkaService(IProducer<Null, string> producer)
        {
            _producer = producer;
        }

        public async Task PublishPermissionEventAsync(IEventBase evt, CancellationToken cancellationToken = default)
        {
            var value = JsonSerializer.Serialize(evt, evt.GetType());
            await _producer.ProduceAsync(Topic, new Message<Null, string> { Value = value }, cancellationToken);
        }

    }
}
