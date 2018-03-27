using AquariumAPI.Controllers;
using AquariumMonitor.APIModels;
using AquariumMonitor.BusinessLogic.Interfaces;
using AquariumMonitor.DAL.Interfaces;
using AquariumMonitor.Models;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.Interfaces;
using Xunit;

namespace AquariumAPI.Tests
{
    public class AquariumControllerTests
    {
        private AquariumController _controller;
        private readonly Mock<ILoggerAdapter<BaseController>> _mockLogger;
        private readonly Mock<IAquariumRepository> _mockAquariumRepository;
        private readonly Mock<IUnitManager> _mockUnitManager;
        private readonly Mock<IAquariumTypeManager> _mockAquariumTypeManager;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IUrlHelper> _mockUrlHelper;

        public AquariumControllerTests()
        {
            // Create mocks
            _mockLogger = new Mock<ILoggerAdapter<BaseController>>();
            _mockAquariumRepository = new Mock<IAquariumRepository>();
            _mockUnitManager = new Mock<IUnitManager>();
            _mockAquariumTypeManager = new Mock<IAquariumTypeManager>();
            _mockMapper = new Mock<IMapper>();
            _mockUrlHelper = new Mock<IUrlHelper>();

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

            _controller = new AquariumController(_mockAquariumRepository.Object, _mockLogger.Object, _mockMapper.Object,
                _mockUnitManager.Object, _mockAquariumTypeManager.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext {User = claimsPrincipal}
                },
                Url = _mockUrlHelper.Object
            };
        }

        [Fact]
        [Trait("Category", "Aquarium Controller Tests")]
        public async Task Get_for_user_returns_AquariumModels()
        {
            //Arrange
            var aquariums = new List<Aquarium>
            {
                new Aquarium
                {
                    Id = 1,
                    Name = "Bedroom",
                    RowVersion = Encoding.ASCII.GetBytes("RowVersion1")
                },
                new Aquarium
                {
                    Id = 2,
                    Name = "Lounge",
                    RowVersion = Encoding.ASCII.GetBytes("RowVersion2")
                }
            };

            var models = new List<AquariumModel>
            {
                new AquariumModel
                { 
                    Url = "https://aquamonitor.com/api/Aquariums/1",
                    Name = "Bedroom",
                    RowVersion = Encoding.ASCII.GetBytes("RowVersion1")
                },
                new AquariumModel
                {
                    Url = "https://aquamonitor.com/api/Aquariums/2",
                    Name = "Lounge",
                    RowVersion = Encoding.ASCII.GetBytes("RowVersion2")
                }
            };

            _mockAquariumRepository.Setup(r => r.GetForUser(1)).ReturnsAsync(aquariums).Verifiable();
            _mockMapper.Setup(am => am.Map<IEnumerable<AquariumModel>>(aquariums)).Returns(models).Verifiable();
            SetupController();

            //Act
            var result = await _controller.GetForUser();

            //Assert
            Assert.Equal(typeof(OkObjectResult), result.GetType());

            var okResult = (OkObjectResult)result;
            Assert.Equal(200, okResult.StatusCode);

            Assert.Equal(typeof(List<AquariumModel>), okResult.Value.GetType());

            var aquariumModels = (List<AquariumModel>)okResult.Value;

            Assert.Equal("Bedroom", aquariumModels[0].Name);
            Assert.Equal("https://aquamonitor.com/api/Aquariums/1", aquariumModels[0].Url);

            Assert.Equal("Lounge", aquariumModels[1].Name);
            Assert.Equal("https://aquamonitor.com/api/Aquariums/2", aquariumModels[1].Url);

            _mockAquariumRepository.Verify(r => r.GetForUser(1), Times.Once);
            _mockMapper.Verify(m => m.Map<IEnumerable<AquariumModel>>(It.IsAny<List<Aquarium>>()), Times.Once);
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

            var aquarium = new Aquarium
            {
                Id = 1,
                Name = "Bedroom",
                RowVersion = Encoding.ASCII.GetBytes("RowVersion")
            };
            _mockAquariumRepository.Setup(r => r.Get(1, 1)).ReturnsAsync(aquarium).Verifiable();
            _mockMapper.Setup(am => am.Map<AquariumModel>(aquarium)).Returns(model).Verifiable();
            SetupController();

            //Act
            var result = await _controller.Get(1);

            //Assert
            Assert.Equal(typeof(OkObjectResult), result.GetType());

            var okResult = (OkObjectResult)result;
            Assert.Equal(200, okResult.StatusCode);

            Assert.Equal(typeof(AquariumModel), okResult.Value.GetType());

            var aquariumModel = (AquariumModel)okResult.Value;
            Assert.Equal("Bedroom", aquariumModel.Name);
            Assert.Equal("https://aquamonitor.com/api/Aquariums/1", aquariumModel.Url);

            _mockAquariumRepository.Verify(r => r.Get(1, 1), Times.Once);
            _mockMapper.Verify(m => m.Map<AquariumModel>(It.IsAny<Aquarium>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "Aquarium Controller Tests")]
        public async Task Get_returns_not_found()
        {
            //Arrange
            _mockAquariumRepository.Setup(r => r.Get(1, 2)).Returns(Task.FromResult<Aquarium>(null)).Verifiable();
            SetupController();

            //Act
            var result = await _controller.Get(2);

            //Assert
            Assert.Equal(typeof(NotFoundResult), result.GetType());

            var notFoundResult = (NotFoundResult)result;
            Assert.Equal(404, notFoundResult.StatusCode);

            _mockAquariumRepository.Verify(r => r.Get(1, 2), Times.Once);
        }

        [Fact]
        [Trait("Category", "Aquarium Controller Tests")]
        public async Task Post_returns_UnprocessableEntity()
        {
            //Arrange
            var model = new AquariumModel();
            SetupController();

            //Act
            var result = await _controller.Post(model);

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
            var aquarium = new Aquarium
            {
                RowVersion = Encoding.ASCII.GetBytes("RowVersion")
            };
            var model = new AquariumModel
            {
                 Url = "http://aqauamonitor.com/api/aquariums/2"
            };
            _mockMapper.Setup(am => am.Map<Aquarium>(It.IsAny<AquariumModel>())).Returns(aquarium);
            _mockMapper.Setup(am => am.Map<AquariumModel>(It.IsAny<Aquarium>())).Returns(model);

            _mockUrlHelper
                .Setup(m => m.Link(It.IsAny<string>(), It.IsAny<object>()))
                .Returns("http://aqauamonitor.com/api/aquariums/1")
                .Verifiable();

            SetupController();

            //Act
            var result = await _controller.Post(model);

            //Assert
            Assert.Equal(typeof(CreatedResult), result.GetType());

            var createdResult = (CreatedResult)result;
            Assert.Equal(201, createdResult.StatusCode);
            Assert.Equal("http://aqauamonitor.com/api/aquariums/1", createdResult.Location);

            var aquariumModel = (AquariumModel)createdResult.Value;
            
            Assert.Equal("http://aqauamonitor.com/api/aquariums/2", aquariumModel.Url);
        }

        [Fact]
        [Trait("Category", "Aquarium Controller Tests")]
        public async Task Post_logs_exception()
        {
            //Arrange
            var model = new AquariumModel();
            var aquarium = new Aquarium
            {
                RowVersion = Encoding.ASCII.GetBytes("RowVersion")
            };
            _mockMapper.Setup(am => am.Map<Aquarium>(It.IsAny<AquariumModel>())).Returns(aquarium).Verifiable();

            var existingAquarium = new Aquarium();
            _mockAquariumRepository.Setup(r => r.Add(It.IsAny<Aquarium>())).Throws(new Exception()).Verifiable();
            _mockLogger.Setup(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>())).Verifiable();

            SetupController();

            //Act
            var result = await _controller.Post(model);

            //Assert
            Assert.Equal(typeof(BadRequestObjectResult), result.GetType());

            var badRequestObjectResult = (BadRequestObjectResult)result;
            Assert.Equal(400, badRequestObjectResult.StatusCode);
            Assert.Equal("Could not create Aquarium", badRequestObjectResult.Value.ToString());

            _mockAquariumRepository.Verify(r => r.Add(It.IsAny<Aquarium>()), Times.Once);

            _mockMapper.Verify(m => m.Map<Aquarium>(It.IsAny<AquariumModel>()), Times.Once);
            _mockLogger.Verify(l => l.Error(It.IsAny<Exception>(), "An error occured whilst trying to create Aquarium."), Times.Once);
        }

        [Fact]
        [Trait("Category", "Aquarium Controller Tests")]
        public async Task Put_returns_not_found()
        {
            //Arrange
            var model = new AquariumModel();
            _mockLogger.Setup(l => l.Warning(It.IsAny<string>())).Verifiable();
            SetupController();

            //Act
            var result = await _controller.Put(2, model);

            //Assert
            Assert.Equal(typeof(NotFoundResult), result.GetType());

            var notFoundResult = (NotFoundResult)result;
            Assert.Equal(404, notFoundResult.StatusCode);
            _mockLogger.Verify(l => l.Warning(It.IsAny<string>()), Times.Once);
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
            _mockMapper.Setup(am => am.Map(It.IsAny<AquariumModel>(), It.IsAny<Aquarium>())).Verifiable();
            _mockMapper.Setup(am => am.Map<AquariumModel>(It.IsAny<Aquarium>())).Returns(model);
            _mockAquariumRepository.Setup(ur => ur.Get(1, 1)).ReturnsAsync(aquarium).Verifiable();

            SetupController();

            //Act
            var result = await _controller.Put(1, model);

            //Assert
            Assert.Equal(typeof(OkObjectResult), result.GetType());

            var okObjectResult = (OkObjectResult)result;
            Assert.Equal(200, okObjectResult.StatusCode);

            _mockAquariumRepository.Verify(r => r.Get(1, 1), Times.Once);
            _mockAquariumRepository.Verify(r => r.Update(It.IsAny<Aquarium>()), Times.Once);

            _mockMapper.Verify(m => m.Map(It.IsAny<AquariumModel>(), It.IsAny<Aquarium>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "Aquarium Controller Tests")]
        public async Task Put_logs_exception()
        {
            //Arrange
            var model = new AquariumModel();
            _mockMapper.Setup(am => am.Map(It.IsAny<AquariumModel>(), It.IsAny<Aquarium>())).Verifiable();

            var existingAquarium = new Aquarium();
            _mockAquariumRepository.Setup(ur => ur.Get(1, 1)).ReturnsAsync(existingAquarium).Verifiable();
            _mockAquariumRepository.Setup(ur => ur.Update(It.IsAny<Aquarium>())).Throws(new Exception()).Verifiable();
            _mockLogger.Setup(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>())).Verifiable();

            SetupController();

            //Act
            var result = await _controller.Put(1, model);

            //Assert
            Assert.Equal(typeof(BadRequestObjectResult), result.GetType());

            var badRequestObjectResult = (BadRequestObjectResult)result;
            Assert.Equal(400, badRequestObjectResult.StatusCode);
            Assert.Equal("Could not update Aquarium", badRequestObjectResult.Value.ToString());

            _mockAquariumRepository.Verify(r => r.Get(1, 1), Times.Once);
            _mockAquariumRepository.Verify(r => r.Update(It.IsAny<Aquarium>()), Times.Once);

            _mockMapper.Verify(m => m.Map(It.IsAny<AquariumModel>(), It.IsAny<Aquarium>()), Times.Once);
            _mockLogger.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "Aquarium Controller Tests")]
        public async Task Delete_returns_ok()
        {
            //Arrange
            var aquarium = new Aquarium
            {
                Id = 1
            };
            _mockAquariumRepository.Setup(r => r.Get(1, 1)).ReturnsAsync(aquarium).Verifiable();
            _mockAquariumRepository.Setup(r => r.Delete(1)).Returns(Task.Delay(0)).Verifiable();
            SetupController();

            //Act
            var result = await _controller.Delete(1);

            //Assert
            Assert.Equal(typeof(OkResult), result.GetType());

            var okResult = (OkResult)result;
            Assert.Equal(200, okResult.StatusCode);

            _mockAquariumRepository.Verify(r => r.Get(1, 1), Times.Once);
            _mockAquariumRepository.Verify(r => r.Delete(1), Times.Once);
        }

        [Fact]
        [Trait("Category", "Aquarium Controller Tests")]
        public async Task Delete_returns_not_found()
        {
            //Arrange
            SetupController();

            //Act
            var result = await _controller.Delete(2);

            //Assert
            Assert.Equal(typeof(NotFoundResult), result.GetType());

            var notFoundResult = (NotFoundResult)result;
            Assert.Equal(404, notFoundResult.StatusCode);

            _mockAquariumRepository.Verify(r => r.Get(1, 2), Times.Once);
        }

        [Fact]
        [Trait("Category", "Aquarium Controller Tests")]
        public async Task Delete_logs_exception()
        {
            //Arrange
            var existingAquarium = new Aquarium();
            _mockAquariumRepository.Setup(r => r.Get(1, 1)).ReturnsAsync(existingAquarium).Verifiable();
            _mockAquariumRepository.Setup(r => r.Delete(1)).Throws(new Exception()).Verifiable();
            _mockLogger.Setup(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>())).Verifiable();
            SetupController();

            //Act
            var result = await _controller.Delete(1);

            //Assert
            Assert.Equal(typeof(BadRequestObjectResult), result.GetType());

            var badRequestObjectResult = (BadRequestObjectResult)result;
            Assert.Equal(400, badRequestObjectResult.StatusCode);

            _mockAquariumRepository.Verify(r => r.Get(1, 1), Times.Once);
            _mockAquariumRepository.Verify(r => r.Delete(1), Times.Once);
            _mockLogger.Verify(l => l.Error(It.IsAny<Exception>(), "An error occured whilst trying to delete Aquarium. AquariumId:1"), Times.Once);
        }
    }
}
