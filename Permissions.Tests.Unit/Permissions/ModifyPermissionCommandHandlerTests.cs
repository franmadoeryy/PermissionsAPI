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
using Permissions.Shared.Kafka.Interfaces;
namespace Permissions.Tests.Unit.Permissions
{
    public class ModifyPermissionCommandHandlerTests
    {
        private readonly Mock<IPermissionRepository> _permissionRepositoryMock;
        private readonly Mock<IPermissionTypeRepository> _permissionTypeRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IPermissionElasticService> _elasticServiceMock;
        private readonly Mock<IPermissionKafkaService> _kafkaServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<ModifyPermissionCommandHandler>> _loggerMock;


        public ModifyPermissionCommandHandlerTests()
        {
            _permissionRepositoryMock = new Mock<IPermissionRepository>();
            _permissionTypeRepositoryMock = new Mock<IPermissionTypeRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _elasticServiceMock = new Mock<IPermissionElasticService>();
            _kafkaServiceMock = new Mock<IPermissionKafkaService>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<ModifyPermissionCommandHandler>>();
        }

        [Fact]
        public async Task Handle_ModifyPermission_ReturnsSucess_WhenDataIsValid()
        {
            //Arrange
            var permission = new Permission
            {
                Id = 1,
                EmployeeName = "Fran",
                EmployeeLastName = "Testing",
                PermissionTypeId = 2,
                PermissionDate = DateTime.UtcNow
            };

            var permissionType = new PermissionType
            {
                Id = 1,
                Description = "Test"
            };

            _permissionRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(permission);
            _permissionTypeRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(permissionType);

            _mapperMock.Setup(m => m.Map<PermissionDocument>(It.IsAny<Permission>())).Returns(new PermissionDocument());
            _mapperMock.Setup(m => m.Map<PermissionDto>(It.IsAny<Permission>())).Returns(new PermissionDto { Id = 1 });


            var command = new ModifyPermissionCommand
            {
                Id = 1,
                EmployeeName = "Leo",
                EmployeeLastName = "Messi",
                PermissionTypeId = 1
            };

            var handler = new ModifyPermissionCommandHandler(
                _permissionRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _elasticServiceMock.Object,
                _kafkaServiceMock.Object,
                _mapperMock.Object,
                _loggerMock.Object,
                _permissionTypeRepositoryMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            _permissionRepositoryMock.Verify(r => r.UpdateAsync(permission), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
            _elasticServiceMock.Verify(e => e.IndexPermissionAsync(It.IsAny<PermissionDocument>(), It.IsAny<CancellationToken>()), Times.Once);
            _kafkaServiceMock.Verify(k => k.PublishPermissionEventAsync(It.IsAny<ModifyPermissionEvent>(), It.IsAny<CancellationToken>()), Times.Once);


        }

        [Fact]
        public async Task Handle_ModifyPermission_ReturnsFailure_WhenPermissionNotFound()
        {
            //Arrange
            _permissionRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Permission)null);


            var command = new ModifyPermissionCommand
            {
                Id = 999,
                EmployeeName = "Leo",
                EmployeeLastName = "Messi",
                PermissionTypeId = 1
            };

            var handler = new ModifyPermissionCommandHandler(
                _permissionRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _elasticServiceMock.Object,
                _kafkaServiceMock.Object,
                _mapperMock.Object,
                _loggerMock.Object,
                _permissionTypeRepositoryMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("not found", result.Error, StringComparison.OrdinalIgnoreCase);
            _permissionRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Permission>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);


        }

        [Fact]
        public async Task Handle_ModifyPermission_ReturnsFailure_WhenPermissionTypeNotFound()
        {
            //Arrange
            _permissionRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new Permission());
            _permissionTypeRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((PermissionType)null);


            var command = new ModifyPermissionCommand
            {
                Id = 1,
                EmployeeName = "Leo",
                EmployeeLastName = "Messi",
                PermissionTypeId = 999
            };

            var handler = new ModifyPermissionCommandHandler(
                _permissionRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _elasticServiceMock.Object,
                _kafkaServiceMock.Object,
                _mapperMock.Object,
                _loggerMock.Object,
                _permissionTypeRepositoryMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("not found", result.Error, StringComparison.OrdinalIgnoreCase);
            _permissionRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Permission>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);


        }

        [Theory]
        [InlineData("", "Messi")]
        [InlineData("Leo", "")]
        [InlineData("", "")]
        public async Task Handle_ModifyPermission_ReturnsFailure_WhenPermissionNameOrLastNameIsEmpty(string employeeName, string employeeLastname)
        {
            //Arrange
            _permissionRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new Permission());
            _permissionTypeRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new PermissionType());


            var command = new ModifyPermissionCommand
            {
                Id = 1,
                EmployeeName = employeeName,
                EmployeeLastName = employeeLastname,
                PermissionTypeId = 2
            };

            var handler = new ModifyPermissionCommandHandler(
                _permissionRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _elasticServiceMock.Object,
                _kafkaServiceMock.Object,
                _mapperMock.Object,
                _loggerMock.Object,
                _permissionTypeRepositoryMock.Object);

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
