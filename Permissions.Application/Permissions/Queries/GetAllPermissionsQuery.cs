using MediatR;
using Permissions.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Permissions.Application.Permissions.Queries
{
    public class GetAllPermissionsQuery : IRequest<List<PermissionDto>>
    {

    }
}
