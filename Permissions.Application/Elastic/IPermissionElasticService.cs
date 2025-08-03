using Permissions.Shared.Elasticsearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Permissions.Application.Elastic
{
    public interface IPermissionElasticService
    {
        Task IndexPermissionAsync(PermissionDocument document, CancellationToken cancellationToken = default);
        Task<List<PermissionDocument>> GetAllPermissionsAsync(CancellationToken cancellationToken = default);
    }
}
