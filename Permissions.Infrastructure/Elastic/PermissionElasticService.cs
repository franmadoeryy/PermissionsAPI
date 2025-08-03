using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Transport;
using Microsoft.Extensions.Logging;
using Permissions.Application.Elastic;
using Permissions.Shared.Elasticsearch;

namespace Permissions.Infrastructure.Elastic
{
    public class PermissionElasticService : IPermissionElasticService
    {
        private readonly ElasticsearchClient _client;
        public PermissionElasticService(ElasticsearchClient client)
        {
            _client = client;
        }

        public async Task<List<PermissionDocument>> GetAllPermissionsAsync(CancellationToken cancellationToken = default)
        {
            var searchResponse = await _client.SearchAsync<PermissionDocument>(s => s
                                    .Index("permissions")
                                    .Query(q => q.MatchAll(new MatchAllQuery())), cancellationToken);

            if (!searchResponse.IsValidResponse)
            {
                if (searchResponse.ElasticsearchServerError?.Error.Type == "index_not_found_exception")
                {
                    return new List<PermissionDocument>();
                }

                throw new Exception("There was an error getting the permissions from Elasticsearch");
            }

            return searchResponse.Documents.ToList();
        }

        public async Task IndexPermissionAsync(PermissionDocument document, CancellationToken cancellationToken = default)
        {
            var response = await _client.IndexAsync(document, idx => idx.Index("permissions").Id(document.PermissionId), cancellationToken);

            if (!response.IsValidResponse)
            {
                throw new Exception("There was an error indexing the permission in Elasticsearch");
            }
        }
    }
}
