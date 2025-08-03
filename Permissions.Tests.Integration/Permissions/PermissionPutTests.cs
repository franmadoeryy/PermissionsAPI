using Microsoft.AspNetCore.Mvc.Testing;
using Permissions.API;
using Permissions.Application.DTOs;
using Permissions.Application.Permissions.Commands;
using Permissions.Shared.Validations;
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
    public class PermissionPutTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;


        public PermissionPutTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task ModifyPermission_ShouldReturnSuccess_WhenDataIsValid()
        {
            // Arrange
            var createCommand = new RequestPermissionCommand
            {
                EmployeeName = "Leo",
                EmployeeLastName = "Messi",
                PermissionTypeId = 1,
                PermissionDate = DateTime.UtcNow
            };

            var createResponse = await _client.PostAsJsonAsync("/api/permissions", createCommand);
            var createdContent = await createResponse.Content.ReadAsStringAsync();
            var createdResult = JsonSerializer.Deserialize<Result<int>>(createdContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.True(createdResult.IsSuccess);
            var id = createdResult.Value;

            
            var command = new ModifyPermissionCommand
            {
                Id = id,
                EmployeeName = "Diego",
                EmployeeLastName = "Maradona",
                PermissionTypeId = 1
            };


            // Act
            var modifyResponse = await _client.PutAsJsonAsync("/api/Permissions", command);
            var modifyContent = await modifyResponse.Content.ReadAsStringAsync();
            var modifyResult = JsonSerializer.Deserialize<Result<PermissionDto>>(modifyContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });


            // Assert
            Assert.Equal(HttpStatusCode.OK, modifyResponse.StatusCode);
            Assert.NotNull(modifyResult);
            Assert.True(modifyResult.IsSuccess);
            Assert.Equal("Diego", modifyResult.Value.EmployeeName);
            Assert.Equal("Maradona", modifyResult.Value.EmployeeLastName);
            Assert.Equal(1, modifyResult.Value.PermissionTypeId);

        }

        [Fact]
        public async Task ModifyPermission_ReturnsNotFound_WhenPermissionDoesNotExist()
        {
            var getResponse = await _client.GetAsync("/api/permissions");
            var getContent = await getResponse.Content.ReadAsStringAsync();
            var permissions = JsonSerializer.Deserialize<List<PermissionDto>>(getContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            int unusedId = (permissions?.Max(p => p.Id) ?? 0) + 10000;


            var modifyCommand = new ModifyPermissionCommand
            {
                Id = unusedId,
                EmployeeName = "Ronaldo",
                EmployeeLastName = "DoesntExist",
                PermissionTypeId = 1
            };

            var response = await _client.PutAsJsonAsync("/api/permissions", modifyCommand);
            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Contains("not found", content, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task ModifyPermission_ReturnsNotFound_WhenPermissionTypeDoesNotExist()
        {

            var createCommand = new RequestPermissionCommand
            {
                EmployeeName = "Spider",
                EmployeeLastName = "Man",
                PermissionTypeId = 2,
                PermissionDate = DateTime.UtcNow
            };

            var createResponse = await _client.PostAsJsonAsync("/api/permissions", createCommand);
            var createdContent = await createResponse.Content.ReadAsStringAsync();
            var createdResult = JsonSerializer.Deserialize<Result<int>>(createdContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var id = createdResult.Value;

            var modifyCommand = new ModifyPermissionCommand
            {
                Id = id,
                EmployeeName = "Bart",
                EmployeeLastName = "Simpson",
                PermissionTypeId = 999999 
            };

            var modifyResponse = await _client.PutAsJsonAsync("/api/permissions", modifyCommand);
            var content = await modifyResponse.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NotFound, modifyResponse.StatusCode);
            Assert.Contains("not found", content, StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData("", "Messi")]
        [InlineData("Leo", "")]
        [InlineData("", "")]
        public async Task ModifyPermission_ReturnsBadRequest_WhenNameOrLastNameIsEmpty(string employeeName, string employeeLastName)
        {
 
            var createCommand = new RequestPermissionCommand
            {
                EmployeeName = "Ricardo",
                EmployeeLastName = "Bochini",
                PermissionTypeId = 1,
                PermissionDate = DateTime.UtcNow
            };

            var createResponse = await _client.PostAsJsonAsync("/api/permissions", createCommand);
            var createdContent = await createResponse.Content.ReadAsStringAsync();
            var createdResult = JsonSerializer.Deserialize<Result<int>>(createdContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var id = createdResult.Value;


            var modifyCommand = new ModifyPermissionCommand
            {
                Id = id,
                EmployeeName = employeeName,
                EmployeeLastName = employeeLastName,
                PermissionTypeId = 1
            };

            var modifyResponse = await _client.PutAsJsonAsync("/api/permissions", modifyCommand);
            var content = await modifyResponse.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, modifyResponse.StatusCode);
            Assert.Contains("cannot be empty", content, StringComparison.OrdinalIgnoreCase);
        }

    }
}
