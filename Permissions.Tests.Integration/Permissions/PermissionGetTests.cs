using Microsoft.AspNetCore.Mvc.Testing;
using Permissions.API;
using Permissions.Application.DTOs;
using Permissions.Application.Permissions.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Permissions.Tests.Integration.Permissions
{
    public class PermissionGetTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;


        public PermissionGetTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetPermissions_ShouldReturnRecentlyAddedPermission()
        {
            // Arrange
            var command = new RequestPermissionCommand
            {
                EmployeeName = "Super",
                EmployeeLastName = "Man",
                PermissionTypeId = 2,
                PermissionDate = DateTime.UtcNow
            };

            var postResponse = await _client.PostAsJsonAsync("/api/permissions", command);
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

            // Wait a little bit to ensure the permission is indexed in Elasticsearch
            await Task.Delay(4000);


            // Act
            var getResponse = await _client.GetAsync("/api/permissions");

            // Assert
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            var content = await getResponse.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<PermissionDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(result);
            Assert.Contains(result, p => p.EmployeeName == "Super" && p.EmployeeLastName == "Man");
        }



    }
}
