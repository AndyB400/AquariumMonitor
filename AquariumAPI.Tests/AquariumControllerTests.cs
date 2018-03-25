using AquariumAPI.Controllers;
using AquariumMonitor.APIModels;
using AquariumMonitor.BusinessLogic.Interfaces;
using AquariumMonitor.DAL.Interfaces;
using AquariumMonitor.Models;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Moq;
using System;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AquariumAPI.Tests
{
    public class AquariumControllerTests
    {
        private AquariumController controller;
        private Mock<ILogger> mockLogger;
        private Mock<IAquariumRepository> mockAquariumRepository;
        private Mock<IUnitManager> mockUnitManager;
        private Mock<IAquariumTypeManager> mockAquariumTypeManager;
        private Mock<IMapper> mockMapper;
        private Mock<IUrlHelper> mockUrlHelper;

        public AquariumControllerTests()
        {
            // Create mocks
            mockLogger = new Mock<ILogger>();
            mockAquariumRepository = new Mock<IAquariumRepository>();
            mockUnitManager = new Mock<IUnitManager>();
            mockAquariumTypeManager = new Mock<IAquariumTypeManager>();
            mockMapper = new Mock<IMapper>();
            mockUrlHelper = new Mock<IUrlHelper>();

            // Setup any generic mock behaviour
        }

        /// <summary>
        /// Build the controller following any changes made to the mocks
        /// </summary>
        private void SetupController()
        {
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
             {
                    new Claim(ClaimTypes.NameIdentifier, "1")
             }));

            controller = new AquariumController(mockAquariumRepository.Object, mockLogger.Object, mockMapper.Object,
                mockUnitManager.Object, mockAquariumTypeManager.Object);
                
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = claimsPrincipal }
            };

            controller.Url = mockUrlHelper.Object;
        }

        [Fact]
        [Trait("Category", "Aquarium Controller Tests")]
        public async Task Get_returns_AquariumModel()
        {
            //Arrange
            var model = new AquariumModel
            {
                Name = "Bedroom",
                Url = "https://aquamonitor.com/api/Aquariums/1"
            };

            var Aquarium = new Aquarium
            {
                Id = 1,
                Name = "Bedroom",
                RowVersion = Encoding.ASCII.GetBytes("RowVersion")
            };
            mockAquariumRepository.Setup(r => r.Get(1, 1)).ReturnsAsync(Aquarium).Verifiable();
            mockMapper.Setup(am => am.Map<AquariumModel>(Aquarium)).Returns(model).Verifiable();
            SetupController();

            //Act
            var result = await controller.Get(1);

            //Assert
            Assert.Equal(typeof(OkObjectResult), result.GetType());

            var okResult = (OkObjectResult)result;
            Assert.Equal(200, okResult.StatusCode);

            Assert.Equal(typeof(AquariumModel), okResult.Value.GetType());

            var AquariumModel = (AquariumModel)okResult.Value;
            Assert.Equal("Bedroom", AquariumModel.Name);
            Assert.Equal("https://aquamonitor.com/api/Aquariums/1", AquariumModel.Url);

            mockAquariumRepository.Verify(r => r.Get(1, 1), Times.Once);
            mockMapper.Verify(m => m.Map<AquariumModel>(It.IsAny<Aquarium>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "Aquarium Controller Tests")]
        public async Task Get_returns_not_found()
        {
            //Arrange
            mockAquariumRepository.Setup(r => r.Get(1, 2)).Returns(Task.FromResult<Aquarium>(null)).Verifiable();
            SetupController();

            //Act
            var result = await controller.Get(2);

            //Assert
            Assert.Equal(typeof(NotFoundResult), result.GetType());

            var notFoundResult = (NotFoundResult)result;
            Assert.Equal(404, notFoundResult.StatusCode);

            mockAquariumRepository.Verify(r => r.Get(1, 2), Times.Once);
        }

        [Fact]
        [Trait("Category", "Aquarium Controller Tests")]
        public async Task Post_returns_UnprocessableEntity()
        {
            //Arrange
            SetupController();
            AquariumModel model = null;

            //Act
            var result = await controller.Post(model);

            //Assert
            Assert.Equal(typeof(StatusCodeResult), result.GetType());

            var statusCodeResult = (StatusCodeResult)result;
            Assert.Equal(422, statusCodeResult.StatusCode);
        }

        [Fact]
        [Trait("Category", "Aquarium Controller Tests")]
        public async Task Post_returns_created()
        {
            //Arrange
            var Aquarium = new Aquarium
            {
                RowVersion = Encoding.ASCII.GetBytes("RowVersion")
            };
            var model = new AquariumModel
            {
                 Url = "http://aqauamonitor.com/api/aquariums/2"
            };
            mockMapper.Setup(am => am.Map<Aquarium>(It.IsAny<AquariumModel>())).Returns(Aquarium);
            mockMapper.Setup(am => am.Map<AquariumModel>(It.IsAny<Aquarium>())).Returns(model);

            mockUrlHelper
                .Setup(m => m.Link(It.IsAny<string>(), It.IsAny<object>()))
                .Returns("http://aqauamonitor.com/api/aquariums/1")
                .Verifiable();

            SetupController();

            //Act
            var result = await controller.Post(null);

            //Assert
            Assert.Equal(typeof(CreatedResult), result.GetType());

            var createdResult = (CreatedResult)result;
            Assert.Equal(201, createdResult.StatusCode);
            Assert.Equal("http://aqauamonitor.com/api/aquariums/1", createdResult.Location);

            var AquariumModel = (AquariumModel)createdResult.Value;
            
            Assert.Equal("http://aqauamonitor.com/api/aquariums/2", AquariumModel.Url);
        }

        [Fact]
        [Trait("Category", "Aquarium Controller Tests")]
        public async Task Put_returns_not_found()
        {
            //Arrange
            var model = new AquariumModel();
            SetupController();

            //Act
            var result = await controller.Put(2, model);

            //Assert
            Assert.Equal(typeof(NotFoundResult), result.GetType());

            var notFoundResult = (NotFoundResult)result;
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        [Trait("Category", "Aquarium Controller Tests")]
        public async Task Put_returns_ok()
        {
            //Arrange
            var aquarium = new Aquarium
            {
                RowVersion = Encoding.ASCII.GetBytes("RowVersion")
            };
            var model = new AquariumModel();
            mockMapper.Setup(am => am.Map(It.IsAny<AquariumModel>(), It.IsAny<Aquarium>())).Verifiable();
            mockMapper.Setup(am => am.Map<AquariumModel>(It.IsAny<Aquarium>())).Returns(model);
            mockAquariumRepository.Setup(ur => ur.Get(1, 1)).ReturnsAsync(aquarium).Verifiable();

            SetupController();

            //Act
            var result = await controller.Put(1, model);

            //Assert
            Assert.Equal(typeof(OkObjectResult), result.GetType());

            var okObjectResult = (OkObjectResult)result;
            Assert.Equal(200, okObjectResult.StatusCode);

            mockAquariumRepository.Verify(r => r.Get(1, 1), Times.Once);
            mockAquariumRepository.Verify(r => r.Update(It.IsAny<Aquarium>()), Times.Once);

            mockMapper.Verify(m => m.Map(It.IsAny<AquariumModel>(), It.IsAny<Aquarium>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "Aquarium Controller Tests")]
        public async Task Put_logs_exception()
        {
            //Arrange
            var Aquarium = new Aquarium();
            var model = new AquariumModel();
            mockMapper.Setup(am => am.Map(It.IsAny<AquariumModel>(), It.IsAny<Aquarium>())).Verifiable();

            var existingAquarium = new Aquarium();
            mockAquariumRepository.Setup(ur => ur.Get(1, 1)).ReturnsAsync(existingAquarium).Verifiable();
            mockAquariumRepository.Setup(ur => ur.Update(It.IsAny<Aquarium>())).Throws(new Exception()).Verifiable();
            mockLogger.Setup(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>())).Verifiable();

            SetupController();

            //Act
            var result = await controller.Put(1, model);

            //Assert
            Assert.Equal(typeof(BadRequestObjectResult), result.GetType());

            var badRequestObjectResult = (BadRequestObjectResult)result;
            Assert.Equal(400, badRequestObjectResult.StatusCode);
            Assert.Equal("Could not update Aquarium", badRequestObjectResult.Value.ToString());

            mockAquariumRepository.Verify(r => r.Get(1, 1), Times.Once);
            mockAquariumRepository.Verify(r => r.Update(It.IsAny<Aquarium>()), Times.Once);

            mockMapper.Verify(m => m.Map(It.IsAny<AquariumModel>(), It.IsAny<Aquarium>()), Times.Once);
            mockLogger.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "Aquarium Controller Tests")]
        public async Task Delete_returns_ok()
        {
            //Arrange
            var Aquarium = new Aquarium
            {
                Id = 1
            };
            mockAquariumRepository.Setup(r => r.Get(1, 1)).ReturnsAsync(Aquarium).Verifiable();
            mockAquariumRepository.Setup(r => r.Delete(1)).Returns(Task.Delay(0)).Verifiable();
            SetupController();

            //Act
            var result = await controller.Delete(1);

            //Assert
            Assert.Equal(typeof(OkResult), result.GetType());

            var okResult = (OkResult)result;
            Assert.Equal(200, okResult.StatusCode);

            mockAquariumRepository.Verify(r => r.Get(1, 1), Times.Once);
            mockAquariumRepository.Verify(r => r.Delete(1), Times.Once);
        }

        [Fact]
        [Trait("Category", "Aquarium Controller Tests")]
        public async Task Delete_returns_not_found()
        {
            //Arrange
            SetupController();

            //Act
            var result = await controller.Delete(2);

            //Assert
            Assert.Equal(typeof(NotFoundResult), result.GetType());

            var notFoundResult = (NotFoundResult)result;
            Assert.Equal(404, notFoundResult.StatusCode);

            mockAquariumRepository.Verify(r => r.Get(1, 2), Times.Once);
        }

        [Fact]
        [Trait("Category", "Aquarium Controller Tests")]
        public async Task Delete_logs_exception()
        {
            //Arrange
            var existingAquarium = new Aquarium();
            mockAquariumRepository.Setup(r => r.Get(1, 1)).ReturnsAsync(existingAquarium).Verifiable();
            mockAquariumRepository.Setup(r => r.Delete(1)).Throws(new Exception()).Verifiable();
            mockLogger.Setup(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>())).Verifiable();
            SetupController();

            //Act
            var result = await controller.Delete(1);

            //Assert
            Assert.Equal(typeof(BadRequestObjectResult), result.GetType());

            var badRequestObjectResult = (BadRequestObjectResult)result;
            Assert.Equal(400, badRequestObjectResult.StatusCode);

            mockAquariumRepository.Verify(r => r.Get(1, 1), Times.Once);
            mockAquariumRepository.Verify(r => r.Delete(1), Times.Once);
            mockLogger.Verify(l => l.Error(It.IsAny<Exception>(), "An error occured whilst trying to delete Aquarium. AquariumId:1"), Times.Once);
        }
    }
}
