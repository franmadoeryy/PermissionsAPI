using Microsoft.EntityFrameworkCore;
using Permissions.Domain.Entities;

namespace Permissions.Infrastructure.Persistance
{
    public class PermissionDbContext : DbContext
    {
        public PermissionDbContext(DbContextOptions<PermissionDbContext> options) : base(options)
        {
        }

        public DbSet<Permission> Permissions { get; set; }
        public DbSet<PermissionType> PermissionTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PermissionType>().HasData(
                new PermissionType { Id = 1, Description = "SuperAdmin" },
                new PermissionType { Id = 2, Description = "Admin" },
                new PermissionType { Id = 3, Description = "User" }
            );

            modelBuilder.Entity<Permission>()
                .HasOne(p => p.PermissionType)
                .WithMany(pt => pt.Permissions)
                .HasForeignKey(p => p.PermissionTypeId);

            base.OnModelCreating(modelBuilder);
        }

    }
}
