using Permissions.Domain.Interfaces;
using Permissions.Infrastructure.Persistance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Permissions.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly PermissionDbContext _context;

        public UnitOfWork(PermissionDbContext context)
        {
            _context = context;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
