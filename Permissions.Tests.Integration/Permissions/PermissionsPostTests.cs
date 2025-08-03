using Azure.Identity;
using Elastic.Clients.Elasticsearch;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Permissions.API;
using Permissions.Application.Permissions.Commands;
using Permissions.Shared.Validations;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Permissions.Tests.Integration.Permissions
{
    public class PermissionsPostTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public PermissionsPostTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task RequestPermission_ShouldCreatePermission_WhenDataIsValid()
        {
            // Arrange
            var command = new RequestPermissionCommand
            {
                EmployeeName = "Leo",
                EmployeeLastName = "Messi",
                PermissionTypeId = 1,
                PermissionDate = DateTime.UtcNow
            };


            // Act
            var response = await _client.PostAsJsonAsync("/api/Permissions", command);
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<Result<int>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });


            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.True(result.Value > 0);

        }

        [Theory]
        [InlineData("", "Messi")]
        [InlineData("Leo", "")]
        [InlineData("", "")]
        public async Task RequestPermission_ReturnsBadRequest_WhenEmployeeNameOrLastNameIsEmpty(string employeeName, string employeeLastName)
        {
            // Arrange
            var command = new RequestPermissionCommand
            {
                EmployeeName = employeeName,
                EmployeeLastName = employeeLastName,
                PermissionTypeId = 1, 
                PermissionDate = DateTime.UtcNow
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/permissions", command);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("cannot be empty", content, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task RequestPermission_ReturnsNotFound_WhenPermissionTypeNotFound()
        {
            // Arrange
            var command = new RequestPermissionCommand
            {
                EmployeeName = "Leo",
                EmployeeLastName = "Messi",
                PermissionTypeId = 999,
                PermissionDate = DateTime.UtcNow
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/permissions", command);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Contains("not found", content, StringComparison.OrdinalIgnoreCase);
        }

    }
}
