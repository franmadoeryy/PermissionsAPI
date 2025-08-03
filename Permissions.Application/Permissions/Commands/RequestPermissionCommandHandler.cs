using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Permissions.Application.DTOs;
using Permissions.Application.Elastic;
using Permissions.Application.Kafka;
using Permissions.Domain.Entities;
using Permissions.Domain.Interfaces;
using Permissions.Shared.Elasticsearch;
using Permissions.Shared.Kafka;
using Permissions.Shared.Validations;


namespace Permissions.Application.Permissions.Commands
{
    public class RequestPermissionCommandHandler : IRequestHandler<RequestPermissionCommand, Result<int>>
    {
        private readonly IPermissionRepository _permissionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPermissionElasticService _permissionElasticService;
        private readonly IPermissionKafkaService _permissionKafkaService;
        private readonly ILogger<RequestPermissionCommandHandler> _logger;
        private readonly IPermissionTypeRepository _permissionTypeRepository;
        private readonly IMapper _mapper;

        public RequestPermissionCommandHandler(IPermissionRepository permissionRepository, IUnitOfWork unitOfWork, IPermissionElasticService permissionElasticService, IPermissionKafkaService permissionKafkaService, ILogger<RequestPermissionCommandHandler> logger, IPermissionTypeRepository permissionTypeRepository, IMapper mapper)
        {
            _permissionRepository = permissionRepository;
            _unitOfWork = unitOfWork;
            _permissionElasticService = permissionElasticService;
            _permissionKafkaService = permissionKafkaService;
            _logger = logger;
            _permissionTypeRepository = permissionTypeRepository;
            _mapper = mapper;
        }

        public async Task<Result<int>> Handle(RequestPermissionCommand command, CancellationToken cancellationToken)
        {
            var permissionType = await _permissionTypeRepository.GetByIdAsync(command.PermissionTypeId);
            if (permissionType == null)
            {
                _logger.LogWarning($"Invalid PermissionTypeId: {command.PermissionTypeId}");
                return Result<int>.Failure($"PermissionType with Id {command.PermissionTypeId} not found");
            }

            if (string.IsNullOrWhiteSpace(command.EmployeeName) || string.IsNullOrWhiteSpace(command.EmployeeLastName))
            {
                _logger.LogWarning("Employee name or last name is empty");
                return Result<int>.Failure("Employee name and last name cannot be empty");
            }

            var permission = new Permission
            {
                EmployeeName = command.EmployeeName,
                EmployeeLastName = command.EmployeeLastName,
                PermissionTypeId = command.PermissionTypeId,
                PermissionDate = command.PermissionDate
            };


            _logger.LogInformation("Saving permission in the database...");
            await _permissionRepository.AddAsync(permission);
            await _unitOfWork.SaveChangesAsync();


            var permissionDocument = _mapper.Map<PermissionDocument>(permission);
            _logger.LogInformation("Indexing new permission in Elasticsearch...");
            await _permissionElasticService.IndexPermissionAsync(permissionDocument, cancellationToken);

            var evt = new RequestPermissionEvent
            {
                Id = Guid.NewGuid(),
                Operation = "request",
                PermissionId= permission.Id,
                EmployeeFullName = $"{permission.EmployeeName} {permission.EmployeeLastName}",
            };

            _logger.LogInformation("Publishing request event to Kafka...");
            await _permissionKafkaService.PublishPermissionEventAsync(evt, cancellationToken);

            return Result<int>.Success(permission.Id);
        }
    }
}
