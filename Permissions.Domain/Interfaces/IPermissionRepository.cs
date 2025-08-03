using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Permissions.Domain.Entities;

namespace Permissions.Domain.Interfaces
{
    public interface IPermissionRepository
    {
        Task<Permission> GetByIdAsync(int id);
        Task AddAsync(Permission permission);
        Task UpdateAsync(Permission permission);
    }
}
