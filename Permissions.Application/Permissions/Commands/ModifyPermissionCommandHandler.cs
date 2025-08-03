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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Permissions.Application.Permissions.Commands
{
    public class ModifyPermissionCommandHandler : IRequestHandler<ModifyPermissionCommand, Result<PermissionDto>>
    {
        private readonly IPermissionRepository _permissionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPermissionElasticService _permissionElasticService;
        private readonly IPermissionKafkaService _permissionKafkaService;
        private readonly IMapper _mapper;
        private readonly ILogger<ModifyPermissionCommandHandler> _logger;
        private readonly IPermissionTypeRepository _permissionTypeRepository;

        public ModifyPermissionCommandHandler(IPermissionRepository permissionRepository, IUnitOfWork unitOfWork, IPermissionElasticService permissionElasticService, IPermissionKafkaService permissionKafkaService, IMapper mapper, ILogger<ModifyPermissionCommandHandler> logger, IPermissionTypeRepository permissionTypeRepository)
        {
            _permissionRepository = permissionRepository;
            _unitOfWork = unitOfWork;
            _permissionElasticService = permissionElasticService;
            _permissionKafkaService = permissionKafkaService;
            _mapper = mapper;
            _logger = logger;
            _permissionTypeRepository = permissionTypeRepository;
        }

        public async Task<Result<PermissionDto>> Handle(ModifyPermissionCommand command, CancellationToken cancellationToken)
        {

            var permission = await _permissionRepository.GetByIdAsync(command.Id);
            if (permission == null)
            {
                _logger.LogWarning($"Tried to modify a permission that does not exist. PermissionId: {command.Id}");
                return Result<PermissionDto>.Failure($"Permission with Id {command.Id} not found" );
            }

            var permissionType = await _permissionTypeRepository.GetByIdAsync(command.PermissionTypeId);
            if (permissionType == null)
            {
                _logger.LogWarning($"Invalid PermissionTypeId: {command.PermissionTypeId} for Id: {command.Id}");
                return Result<PermissionDto>.Failure($"PermissionType with Id {command.PermissionTypeId} not found");
            }

            if (string.IsNullOrWhiteSpace(command.EmployeeName) || string.IsNullOrWhiteSpace(command.EmployeeLastName))
            {
                _logger.LogWarning("Employee name or last name is empty.");
                return Result<PermissionDto>.Failure("Employee name and last name cannot be empty");
            }

            permission.EmployeeName = command.EmployeeName;
            permission.EmployeeLastName = command.EmployeeLastName;
            permission.PermissionTypeId = command.PermissionTypeId;
            permission.PermissionDate = DateTime.Now;

            _logger.LogInformation("Modifying permission in the database...");
            await _permissionRepository.UpdateAsync(permission);
            await _unitOfWork.SaveChangesAsync();


            var permissionDocument = _mapper.Map<PermissionDocument>(permission);

           
            _logger.LogInformation("Indexing modified permission in Elasticsearch...");
            await _permissionElasticService.IndexPermissionAsync(permissionDocument, cancellationToken);



            var evt = new ModifyPermissionEvent
            {
                Id = Guid.NewGuid(),
                Operation = "modify",
                PermissionId = permission.Id,
                EmployeeName = permission.EmployeeName,
                EmployeeLastName = permission.EmployeeLastName,
                PermissionTypeId = permission.PermissionTypeId,
                PermissionDate = permission.PermissionDate
            };

            _logger.LogInformation("Publishing modify event to Kafka...");
            await _permissionKafkaService.PublishPermissionEventAsync(evt, cancellationToken);


            return Result<PermissionDto>.Success(_mapper.Map<PermissionDto>(permission));
        }
    }
}
