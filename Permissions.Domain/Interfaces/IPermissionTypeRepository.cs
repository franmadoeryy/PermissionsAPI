using Permissions.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Permissions.Domain.Interfaces
{
    public interface IPermissionTypeRepository
    {
        Task<PermissionType> GetByIdAsync(int id);
    }
}
