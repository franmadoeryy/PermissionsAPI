using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Permissions.Application.DTOs;
using Permissions.Application.Elastic;
using Permissions.Application.Kafka;
using Permissions.Domain.Entities;
using Permissions.Shared.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Permissions.Application.Permissions.Queries
{
    public class GetAllPermissionsQueryHandler : IRequestHandler<GetAllPermissionsQuery, List<PermissionDto>>
    {
        private readonly IPermissionElasticService _elasticService;
        private readonly IMapper _mapper;
        private readonly IPermissionKafkaService _permissionKafkaService;
        private readonly ILogger<GetAllPermissionsQueryHandler> _logger;

        public GetAllPermissionsQueryHandler(IPermissionElasticService elasticService, IMapper mapper, IPermissionKafkaService permissionKafkaService, ILogger<GetAllPermissionsQueryHandler> logger)
        {
            _elasticService = elasticService;
            _mapper = mapper;
            _permissionKafkaService = permissionKafkaService;
            _logger = logger;
        }


        public async Task<List<PermissionDto>> Handle(GetAllPermissionsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting all the permisions from Elasticsearch...");
            var docs = await _elasticService.GetAllPermissionsAsync(cancellationToken);


            var evt = new GetPermissionsEvent
            {
                Id = Guid.NewGuid(),
                Operation = "get",
                PermissionIds = docs.Select(p => p.PermissionId).ToList(),
                
            };

            _logger.LogInformation("Publishing get event to Kafka...");
            await _permissionKafkaService.PublishPermissionEventAsync(evt, cancellationToken);


            return _mapper.Map<List<PermissionDto>>(docs);
        }
    }
}
