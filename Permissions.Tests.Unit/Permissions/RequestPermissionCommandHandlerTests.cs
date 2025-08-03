using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using Permissions.Application.DTOs;
using Permissions.Application.Elastic;
using Permissions.Application.Kafka;
using Permissions.Application.Permissions.Commands;
using Permissions.Domain.Entities;
using Permissions.Domain.Interfaces;
using Permissions.Shared.Elasticsearch;
using Permissions.Shared.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Permissions.Tests.Unit.Permissions
{
    public class RequestPermissionCommandHandlerTests
    {
        private readonly Mock<IPermissionRepository> _permissionRepositoryMock;
        private readonly Mock<IPermissionTypeRepository> _permissionTypeRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IPermissionElasticService> _elasticServiceMock;
        private readonly Mock<IPermissionKafkaService> _kafkaServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<RequestPermissionCommandHandler>> _loggerMock;


        public RequestPermissionCommandHandlerTests()
        {
            _permissionRepositoryMock = new Mock<IPermissionRepository>();
            _permissionTypeRepositoryMock = new Mock<IPermissionTypeRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _elasticServiceMock = new Mock<IPermissionElasticService>();
            _kafkaServiceMock = new Mock<IPermissionKafkaService>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<RequestPermissionCommandHandler>>();
        }

        [Fact]
        public async Task Handle_RequestPermission_ReturnsSucess_WhenDataIsValid()
        {
            //Arrange
            var permissionType = new PermissionType
            {
                Id = 1,
                Description = "Test"
            };

            _permissionTypeRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(permissionType);

            _mapperMock.Setup(m => m.Map<PermissionDocument>(It.IsAny<Permission>())).Returns(new PermissionDocument());

            var permissionToAdd = (Permission)null;

            _permissionRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Permission>()))
                                        .Callback<Permission>(p => permissionToAdd = p)
                                        .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Callback(() =>
            {
                if (permissionToAdd != null)
                    permissionToAdd.Id = 10;
            });

            var command = new RequestPermissionCommand
            {
                EmployeeName = "Leo",
                EmployeeLastName = "Messi",
                PermissionTypeId = 1,
                PermissionDate = DateTime.UtcNow
            };

            var handler = new RequestPermissionCommandHandler(
                _permissionRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _elasticServiceMock.Object,
                _kafkaServiceMock.Object,                
                _loggerMock.Object,
                _permissionTypeRepositoryMock.Object,
                _mapperMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(10, result.Value);
            _permissionRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Permission>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
            _elasticServiceMock.Verify(e => e.IndexPermissionAsync(It.IsAny<PermissionDocument>(), It.IsAny<CancellationToken>()), Times.Once);
            _kafkaServiceMock.Verify(k => k.PublishPermissionEventAsync(It.IsAny<RequestPermissionEvent>(), It.IsAny<CancellationToken>()), Times.Once);


        }

        [Fact]
        public async Task Handle_RequestPermission_ReturnsFailure_WhenPermissionTypeNotFound()
        {
            //Arrange
            _permissionTypeRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((PermissionType)null);
           
            var command = new RequestPermissionCommand
            {
                EmployeeName = "Leo",
                EmployeeLastName = "Messi",
                PermissionTypeId = 999,
                PermissionDate = DateTime.UtcNow
            };

            var handler = new RequestPermissionCommandHandler(
                _permissionRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _elasticServiceMock.Object,
                _kafkaServiceMock.Object,
                _loggerMock.Object,
                _permissionTypeRepositoryMock.Object,
                _mapperMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("not found", result.Error, StringComparison.OrdinalIgnoreCase);
            _permissionRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Permission>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }


        [Theory]
        [InlineData("", "Messi")]
        [InlineData("Leo", "")]
        [InlineData("", "")]
        public async Task Handle_RequestPermission_ReturnsFailure_WhenPermissionNameOrLastNameIsEmpty(string employeeName, string employeeLastname)
        {
            //Arrange
            _permissionTypeRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new PermissionType());


            var command = new RequestPermissionCommand
            {
                EmployeeName = employeeName,
                EmployeeLastName = employeeLastname,
                PermissionTypeId = 2
            };

            var handler = new RequestPermissionCommandHandler(
                _permissionRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _elasticServiceMock.Object,
                _kafkaServiceMock.Object,
                _loggerMock.Object,
                _permissionTypeRepositoryMock.Object,
                _mapperMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("cannot be empty", result.Error, StringComparison.OrdinalIgnoreCase);
            _permissionRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Permission>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);


        }
    }
}
