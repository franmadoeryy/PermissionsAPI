using MediatR;
using Permissions.Application.DTOs;
using Permissions.Domain.Entities;
using Permissions.Shared.Validations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Permissions.Application.Permissions.Commands
{
    public class ModifyPermissionCommand : IRequest<Result<PermissionDto>>
    {
        public int Id { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeLastName { get; set; }
        public int PermissionTypeId { get; set; }
        
    }
}
