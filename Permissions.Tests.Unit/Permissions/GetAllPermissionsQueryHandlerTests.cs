using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using Permissions.Application.DTOs;
using Permissions.Application.Elastic;
using Permissions.Application.Kafka;
using Permissions.Application.Permissions.Commands;
using Permissions.Application.Permissions.Queries;
using Permissions.Shared.Elasticsearch;
using Permissions.Shared.Kafka;


namespace Permissions.Tests.Unit.Permissions
{
    public class GetAllPermissionsQueryHandlerTests
    {
        private readonly Mock<IPermissionElasticService> _elasticServiceMock;
        private readonly Mock<IPermissionKafkaService> _kafkaServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<GetAllPermissionsQueryHandler>> _loggerMock;

        public GetAllPermissionsQueryHandlerTests()
        {
            _elasticServiceMock = new Mock<IPermissionElasticService>();
            _kafkaServiceMock = new Mock<IPermissionKafkaService>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<GetAllPermissionsQueryHandler>>();
        }

        [Fact]
        public async Task Handle_ReturnsMappedListAndPublishesEvent()
        {
            // Arrange
            var permissionDocs = new List<PermissionDocument>
            {
                new PermissionDocument { PermissionId = 1, EmployeeName = "Leo", EmployeeLastName = "Messi" },
                new PermissionDocument { PermissionId = 2, EmployeeName = "Fran", EmployeeLastName = "Test" }
            };

            var mapped = new List<PermissionDto>
            {
                new PermissionDto { Id = 1, EmployeeName = "Leo", EmployeeLastName = "Messi" },
                new PermissionDto { Id = 2, EmployeeName = "Fran", EmployeeLastName = "Test" }
            };

            _elasticServiceMock.Setup(x => x.GetAllPermissionsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(permissionDocs);

            _mapperMock.Setup(x => x.Map<List<PermissionDto>>(permissionDocs)).Returns(mapped);

            var handler = new GetAllPermissionsQueryHandler(
                _elasticServiceMock.Object,
                _mapperMock.Object,
                _kafkaServiceMock.Object,
                _loggerMock.Object
            );

            // Act
            var result = await handler.Handle(new GetAllPermissionsQuery(), CancellationToken.None);


            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].Id);
            _elasticServiceMock.Verify(x => x.GetAllPermissionsAsync(It.IsAny<CancellationToken>()), Times.Once);
            _kafkaServiceMock.Verify(x => x.PublishPermissionEventAsync(It.IsAny<GetPermissionsEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(x => x.Map<List<PermissionDto>>(permissionDocs), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsEmptyList_WhenElasticReturnsEmpty()
        {
            // Arrange
            _elasticServiceMock.Setup(x => x.GetAllPermissionsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PermissionDocument>());

            _mapperMock.Setup(x => x.Map<List<PermissionDto>>(It.IsAny<List<PermissionDocument>>()))
                .Returns(new List<PermissionDto>());

            var handler = new GetAllPermissionsQueryHandler(
                _elasticServiceMock.Object,
                _mapperMock.Object,
                _kafkaServiceMock.Object,
                _loggerMock.Object
            );

            // Act
            var result = await handler.Handle(new GetAllPermissionsQuery(), CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _elasticServiceMock.Verify(x => x.GetAllPermissionsAsync(It.IsAny<CancellationToken>()), Times.Once);
            _kafkaServiceMock.Verify(x => x.PublishPermissionEventAsync(It.IsAny<GetPermissionsEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(x => x.Map<List<PermissionDto>>(It.IsAny<List<PermissionDocument>>()), Times.Once);
        }


    }
}
