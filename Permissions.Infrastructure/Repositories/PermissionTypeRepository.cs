using Microsoft.EntityFrameworkCore;
using Permissions.Domain.Entities;
using Permissions.Domain.Interfaces;
using Permissions.Infrastructure.Persistance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Permissions.Infrastructure.Repositories
{
    public class PermissionTypeRepository : IPermissionTypeRepository
    {
        private readonly PermissionDbContext _context;

        public PermissionTypeRepository(PermissionDbContext context)
        {
            _context = context;
        }

        public async Task<PermissionType> GetByIdAsync(int id)
        {
            return await _context.PermissionTypes.FindAsync(id);
        }
    }
}
