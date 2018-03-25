using AquariumAPI.Controllers;
using System;
using Microsoft.Extensions.Configuration;
using Serilog;
using Xunit;
using Moq;
using AquariumMonitor.DAL.Interfaces;
using AquariumMonitor.BusinessLogic.Interfaces;
using AutoMapper;
using Pwned;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AquariumMonitor.Models.APIModels;
using AquariumMonitor.Models;
using System.Text;
using System.Security.Claims;

namespace AquariumAPI.Tests
{
    public class UserControllerTests
    {
        private UserController controller;
        private Mock<IConfiguration> mockConfiguration;
        private Mock<ILogger> mockLogger;
        private Mock<IUserRepository> mockUserRepository;
        private Mock<IPasswordManager> mockPasswordManager;
        private Mock<IPasswordRepository> mockPasswordRepository;
        private Mock<IMapper> mockMapper;
        private Mock<IHaveIBeenPwnedRestClient>  mockPwnedClient;
        private Mock<IUrlHelper> mockUrlHelper;

        public UserControllerTests()
        {
            // Create mocks
            mockConfiguration = new Mock<IConfiguration>();
            mockLogger = new Mock<ILogger>();
            mockUserRepository = new Mock<IUserRepository>();
            mockPasswordManager = new Mock<IPasswordManager>();
            mockPasswordRepository = new Mock<IPasswordRepository>();
            mockMapper = new Mock<IMapper>();
            mockPwnedClient = new Mock<IHaveIBeenPwnedRestClient>();
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

            controller = new UserController(mockConfiguration.Object, mockLogger.Object, mockUserRepository.Object,
                mockPasswordManager.Object, mockPasswordRepository.Object, mockMapper.Object, mockPwnedClient.Object);

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = claimsPrincipal }
            };

            controller.Url = mockUrlHelper.Object;
        }

        [Fact]
        [Trait("Category", "User Controller Tests")]
        public async Task Get_returns_UserModel()
        {
            //Arrange
            var model = new UserModel
            {
                Username = "andyb",
                Email = "andy@aquamonitor.com",
                FirstName = "Andy",
                LastName = "Bradford",
                Password = "SuperSecretPassword",
                Url = "https://aquamonitor.com/api/users/1"
            };

            var user = new User
            {
                Id = 1,
                Username = "andyb",
                Email = "andy@aquamonitor.com",
                FirstName = "Andy",
                LastName = "Bradford",
                Password = "SuperSecretPassword",
                LastLogin = new DateTimeOffset(new DateTime(2017, 1, 1)),
                RowVersion = Encoding.ASCII.GetBytes("RowVersion")
            };
            mockUserRepository.Setup(r => r.Get(1)).ReturnsAsync(user).Verifiable();
            mockMapper.Setup(am => am.Map<UserModel>(user)).Returns(model).Verifiable();
            SetupController();

            //Act
            var result = await controller.Get(1);

            //Assert
            Assert.Equal(typeof(OkObjectResult), result.GetType());

            var okResult = (OkObjectResult)result;
            Assert.Equal(200, okResult.StatusCode);

            Assert.Equal(typeof(UserModel), okResult.Value.GetType());

            var userModel = (UserModel)okResult.Value;
            Assert.Equal("andyb", userModel.Username);
            Assert.Equal("Andy", userModel.FirstName);
            Assert.Equal("Bradford", userModel.LastName);
            Assert.Equal("andy@aquamonitor.com", userModel.Email);
            Assert.Equal("SuperSecretPassword", userModel.Password);
            Assert.Equal("https://aquamonitor.com/api/users/1", userModel.Url);

            mockUserRepository.Verify(r => r.Get(1), Times.Once);
            mockMapper.Verify(m => m.Map<UserModel>(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "User Controller Tests")]
        public async Task Get_returns_not_found()
        {
            //Arrange
            mockUserRepository.Setup(r => r.Get(2)).Returns(Task.FromResult<User>(null)).Verifiable();
            SetupController();

            //Act
            var result = await controller.Get(2);

            //Assert
            Assert.Equal(typeof(NotFoundResult), result.GetType());

            var notFoundResult = (NotFoundResult)result;
            Assert.Equal(404, notFoundResult.StatusCode);

            mockUserRepository.Verify(r => r.Get(2), Times.Once);
        }

        [Fact]
        [Trait("Category", "User Controller Tests")]
        public async Task Post_returns_UnprocessableEntity()
        {
            //Arrange
            SetupController();
            UserModel model = null;

            //Act
            var result = await controller.Post(model);

            //Assert
            Assert.Equal(typeof(StatusCodeResult), result.GetType());

            var statusCodeResult = (StatusCodeResult)result;
            Assert.Equal(422, statusCodeResult.StatusCode);
        }

        [Fact]
        [Trait("Category", "User Controller Tests")]
        public async Task Post_no_password_returns_bad_request()
        {
            //Arrange
            var userNoPassword = new User
            {
                FirstName = "andy"
            };
            mockMapper.Setup(am => am.Map<User>(It.IsAny<UserModel>())).Returns(userNoPassword);
            SetupController();

            //Act
            var result = await controller.Post(null);

            //Assert
            Assert.Equal(typeof(BadRequestObjectResult), result.GetType());

            var badRequestObjectResult = (BadRequestObjectResult)result;
            Assert.Equal(400, badRequestObjectResult.StatusCode);
            Assert.Equal("Password cannot be null", badRequestObjectResult.Value.ToString());
        }

        [Fact]
        [Trait("Category", "User Controller Tests")]
        public async Task Post_user_already_exists_returns_bad_request()
        {
            //Arrange
            var user = new User
            {
                Password = "Password"
            };
            mockMapper.Setup(am => am.Map<User>(It.IsAny<UserModel>())).Returns(user);

            mockPwnedClient.Setup(pc => pc.IsPasswordPwned(It.IsAny<string>())).ReturnsAsync(false);

            var existingUser = new User();
            mockUserRepository.Setup(ur => ur.Get(It.IsAny<string>())).ReturnsAsync(existingUser);
            SetupController();

            //Act
            var result = await controller.Post(null);

            //Assert
            Assert.Equal(typeof(BadRequestObjectResult), result.GetType());

            var badRequestObjectResult = (BadRequestObjectResult)result;
            Assert.Equal(400, badRequestObjectResult.StatusCode);
            Assert.Equal("User already exists", badRequestObjectResult.Value.ToString());
        }

        [Fact]
        [Trait("Category", "User Controller Tests")]
        public async Task Post_returns_created()
        {
            //Arrange
            var user = new User
            {
                Password = "Password"
            };
            var model = new UserModel
            {
                Password = "Password",
                 Url = "http://aqauamonitor.com/api/users/2"
            };
            mockMapper.Setup(am => am.Map<User>(It.IsAny<UserModel>())).Returns(user);
            mockMapper.Setup(am => am.Map<UserModel>(It.IsAny<User>())).Returns(model);

            mockPwnedClient.Setup(pc => pc.IsPasswordPwned(It.IsAny<string>())).ReturnsAsync(false);

            mockUrlHelper
                .Setup(m => m.Link(It.IsAny<string>(), It.IsAny<object>()))
                .Returns("http://aqauamonitor.com/api/users/1")
                .Verifiable();

            SetupController();

            //Act
            var result = await controller.Post(null);

            //Assert
            Assert.Equal(typeof(CreatedResult), result.GetType());

            var createdResult = (CreatedResult)result;
            Assert.Equal(201, createdResult.StatusCode);
            Assert.Equal("http://aqauamonitor.com/api/users/1", createdResult.Location);

            var userModel = (UserModel)createdResult.Value;
            
            Assert.Equal("http://aqauamonitor.com/api/users/2", userModel.Url);
        }

        [Fact]
        [Trait("Category", "User Controller Tests")]
        public async Task Put_returns_not_found()
        {
            //Arrange
            var model = new UserModel();
            SetupController();

            //Act
            var result = await controller.Put(2, model);

            //Assert
            Assert.Equal(typeof(NotFoundResult), result.GetType());

            var notFoundResult = (NotFoundResult)result;
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        [Trait("Category", "User Controller Tests")]
        public async Task Put_returns_ok()
        {
            //Arrange
            var user = new User();
            var model = new UserModel();
            mockMapper.Setup(am => am.Map(It.IsAny<UserModel>(), It.IsAny<User>())).Verifiable();
            mockMapper.Setup(am => am.Map<UserModel>(It.IsAny<User>())).Returns(model);

            var existingUser = new User();
            mockUserRepository.Setup(ur => ur.Get(It.IsAny<int>())).ReturnsAsync(existingUser).Verifiable();

            SetupController();

            //Act
            var result = await controller.Put(1, model);

            //Assert
            Assert.Equal(typeof(OkObjectResult), result.GetType());

            var okObjectResult = (OkObjectResult)result;
            Assert.Equal(200, okObjectResult.StatusCode);

            mockUserRepository.Verify(r => r.Get(1), Times.Once);
            mockUserRepository.Verify(r => r.Update(It.IsAny<User>()), Times.Once);

            mockMapper.Verify(m => m.Map(It.IsAny<UserModel>(), It.IsAny<User>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "User Controller Tests")]
        public async Task Put_logs_exception()
        {
            //Arrange
            var user = new User();
            var model = new UserModel();
            mockMapper.Setup(am => am.Map(It.IsAny<UserModel>(), It.IsAny<User>())).Verifiable();

            var existingUser = new User();
            mockUserRepository.Setup(ur => ur.Get(It.IsAny<int>())).ReturnsAsync(existingUser).Verifiable();
            mockUserRepository.Setup(ur => ur.Update(It.IsAny<User>())).Throws(new Exception()).Verifiable();

            SetupController(); 

            //Act
            var result = await controller.Put(1, model);

            //Assert
            Assert.Equal(typeof(BadRequestObjectResult), result.GetType());

            var badRequestObjectResult = (BadRequestObjectResult)result;
            Assert.Equal(400, badRequestObjectResult.StatusCode);
            Assert.Equal("Could not update User", badRequestObjectResult.Value.ToString());

            mockUserRepository.Verify(r => r.Get(1), Times.Once);
            mockUserRepository.Verify(r => r.Update(It.IsAny<User>()), Times.Once);

            mockMapper.Verify(m => m.Map(It.IsAny<UserModel>(), It.IsAny<User>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "User Controller Tests")]
        public async Task Delete_returns_ok()
        {
            //Arrange
            var user = new User
            {
                Id = 1
            };
            mockUserRepository.Setup(r => r.Get(1)).ReturnsAsync(user).Verifiable();
            mockUserRepository.Setup(r => r.Delete(1)).Returns(Task.Delay(0)).Verifiable();
            SetupController();

            //Act
            var result = await controller.Delete(1);

            //Assert
            Assert.Equal(typeof(OkResult), result.GetType());

            var okResult = (OkResult)result;
            Assert.Equal(200, okResult.StatusCode);

            mockUserRepository.Verify(r => r.Get(1), Times.Once);
            mockUserRepository.Verify(r => r.Delete(1), Times.Once);
        }

        [Fact]
        [Trait("Category", "User Controller Tests")]
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

            mockUserRepository.Verify(r => r.Get(2), Times.Once);
        }
    }
}
