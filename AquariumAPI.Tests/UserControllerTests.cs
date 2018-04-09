using AquariumAPI.Controllers;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Microsoft.Extensions.Configuration;
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
using BusinessLogic.Interfaces;

namespace AquariumAPI.Tests
{
    [ExcludeFromCodeCoverage]
    public class UserControllerTests
    {
        private UserController _controller;
        private readonly Mock<ILoggerAdapter<BaseController>> _mockLogger;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IPasswordManager> _mockPasswordManager;
        private readonly Mock<IPasswordRepository> _mockPasswordRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IHaveIBeenPwnedRestClient>  _mockPwnedClient;
        private readonly Mock<IUrlHelper> _mockUrlHelper;

        public UserControllerTests()
        {
            // Create mocks
            _mockLogger = new Mock<ILoggerAdapter<BaseController>>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockPasswordManager = new Mock<IPasswordManager>();
            _mockPasswordRepository = new Mock<IPasswordRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockPwnedClient = new Mock<IHaveIBeenPwnedRestClient>();
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

            _controller = new UserController(_mockLogger.Object, _mockUserRepository.Object,
                _mockPasswordManager.Object, _mockPasswordRepository.Object, _mockMapper.Object, _mockPwnedClient.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = claimsPrincipal }
                },

                Url = _mockUrlHelper.Object
            };
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
            _mockUserRepository.Setup(r => r.Get(1)).ReturnsAsync(user).Verifiable();
            _mockMapper.Setup(am => am.Map<UserModel>(user)).Returns(model).Verifiable();
            SetupController();

            //Act
            var result = await _controller.Get(1);

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

            _mockUserRepository.Verify(r => r.Get(1), Times.Once);
            _mockMapper.Verify(m => m.Map<UserModel>(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "User Controller Tests")]
        public async Task Get_returns_not_found()
        {
            //Arrange
            _mockUserRepository.Setup(r => r.Get(2)).Returns(Task.FromResult<User>(null)).Verifiable();
            SetupController();

            //Act
            var result = await _controller.Get(2);

            //Assert
            Assert.Equal(typeof(NotFoundResult), result.GetType());

            var notFoundResult = (NotFoundResult)result;
            Assert.Equal(404, notFoundResult.StatusCode);

            _mockUserRepository.Verify(r => r.Get(2), Times.Once);
        }

        [Fact]
        [Trait("Category", "User Controller Tests")]
        public async Task Post_returns_UnprocessableEntity()
        {
            //Arrange
            var model = new UserModel();
            SetupController();

            //Act
            var result = await _controller.Post(model);

            //Assert
            Assert.Equal(typeof(StatusCodeResult), result.GetType());

            var statusCodeResult = (StatusCodeResult)result;
            Assert.Equal(422, statusCodeResult.StatusCode);
        }

        [Fact]
        [Trait("Category", "User Controller Tests")]
        public async Task Post_returns_pwned_password()
        {
            //Arrange
            var model = new UserModel();
            _mockPwnedClient.Setup(c => c.IsPasswordPwned(It.IsAny<string>())).ReturnsAsync(true);

            var user = new User
            {
                FirstName = "andy",
                Password = "abc123"
            };
            _mockMapper.Setup(am => am.Map<User>(It.IsAny<UserModel>())).Returns(user);

            SetupController();

            //Act
            var result = await _controller.Post(model);

            //Assert
            Assert.Equal(typeof(BadRequestObjectResult), result.GetType());

            var badRequestObjectResult = (BadRequestObjectResult)result;
            Assert.Equal("Pwned Password", badRequestObjectResult.Value.ToString());
        }

        [Fact]
        [Trait("Category", "User Controller Tests")]
        public async Task Post_no_password_returns_bad_request()
        {
            //Arrange
            var model = new UserModel();

            var userNoPassword = new User
            {
                FirstName = "andy"
            };
            _mockMapper.Setup(am => am.Map<User>(It.IsAny<UserModel>())).Returns(userNoPassword);
            SetupController();

            //Act
            var result = await _controller.Post(model);

            //Assert
            Assert.Equal(typeof(BadRequestObjectResult), result.GetType());

            var badRequestObjectResult = (BadRequestObjectResult)result;
            Assert.Equal("Password cannot be null", badRequestObjectResult.Value.ToString());
        }

        [Fact]
        [Trait("Category", "User Controller Tests")]
        public async Task Post_user_already_exists_returns_bad_request()
        {
            //Arrange
            var model = new UserModel();
            var user = new User
            {
                Password = "Password"
            };
            _mockMapper.Setup(am => am.Map<User>(It.IsAny<UserModel>())).Returns(user);

            _mockPwnedClient.Setup(pc => pc.IsPasswordPwned(It.IsAny<string>())).ReturnsAsync(false);

            var existingUser = new User();
            _mockUserRepository.Setup(ur => ur.Get(It.IsAny<string>())).ReturnsAsync(existingUser);
            SetupController();

            //Act
            var result = await _controller.Post(model);

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
                Password = "Password",
                RowVersion = Encoding.ASCII.GetBytes("RowVersion")
            };
            var model = new UserModel
            {
                Password = "Password",
                 Url = "http://aqauamonitor.com/api/users/2"
            };
            _mockMapper.Setup(am => am.Map<User>(It.IsAny<UserModel>())).Returns(user);
            _mockMapper.Setup(am => am.Map<UserModel>(It.IsAny<User>())).Returns(model);

            _mockPwnedClient.Setup(pc => pc.IsPasswordPwned(It.IsAny<string>())).ReturnsAsync(false);

            _mockUrlHelper
                .Setup(m => m.Link(It.IsAny<string>(), It.IsAny<object>()))
                .Returns("http://aqauamonitor.com/api/users/1")
                .Verifiable();

            SetupController();

            //Act
            var result = await _controller.Post(model);

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
            var result = await _controller.Put(2, model);

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
            var user = new User
            {
                RowVersion = Encoding.ASCII.GetBytes("RowVersion")
            };
            var model = new UserModel();
            _mockMapper.Setup(am => am.Map(It.IsAny<UserModel>(), It.IsAny<User>())).Verifiable();
            _mockMapper.Setup(am => am.Map<UserModel>(It.IsAny<User>())).Returns(model);
            _mockUserRepository.Setup(ur => ur.Get(It.IsAny<int>())).ReturnsAsync(user).Verifiable();

            SetupController();

            //Act
            var result = await _controller.Put(1, model);

            //Assert
            Assert.Equal(typeof(OkObjectResult), result.GetType());

            var okObjectResult = (OkObjectResult)result;
            Assert.Equal(200, okObjectResult.StatusCode);

            _mockUserRepository.Verify(r => r.Get(1), Times.Once);
            _mockUserRepository.Verify(r => r.Update(It.IsAny<User>()), Times.Once);

            _mockMapper.Verify(m => m.Map(It.IsAny<UserModel>(), It.IsAny<User>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "User Controller Tests")]
        public async Task Put_logs_exception()
        {
            //Arrange
            var user = new User();
            var model = new UserModel();
            _mockMapper.Setup(am => am.Map(It.IsAny<UserModel>(), It.IsAny<User>())).Verifiable();

            var existingUser = new User();
            _mockUserRepository.Setup(ur => ur.Get(It.IsAny<int>())).ReturnsAsync(existingUser).Verifiable();
            _mockUserRepository.Setup(ur => ur.Update(It.IsAny<User>())).Throws(new Exception()).Verifiable();

            SetupController(); 

            //Act
            var result = await _controller.Put(1, model);

            //Assert
            Assert.Equal(typeof(BadRequestObjectResult), result.GetType());

            var badRequestObjectResult = (BadRequestObjectResult)result;
            Assert.Equal(400, badRequestObjectResult.StatusCode);
            Assert.Equal("Could not update User", badRequestObjectResult.Value.ToString());

            _mockUserRepository.Verify(r => r.Get(1), Times.Once);
            _mockUserRepository.Verify(r => r.Update(It.IsAny<User>()), Times.Once);

            _mockMapper.Verify(m => m.Map(It.IsAny<UserModel>(), It.IsAny<User>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "User Controller Tests")]
        public async Task Put_invalid_etag_returns_precondition_failed()
        {
            //Arrange
            var model = new UserModel();
            _mockMapper.Setup(am => am.Map(It.IsAny<UserModel>(), It.IsAny<User>())).Verifiable();

            var existingUser = new User
            {
                RowVersion = Encoding.ASCII.GetBytes("A Different RowVersion")
            };
            _mockUserRepository.Setup(ur => ur.Get(1)).ReturnsAsync(existingUser).Verifiable();

            SetupController();

            //Act
            var result = await _controller.Put(1, model);

            //Assert
            Assert.Equal(typeof(StatusCodeResult), result.GetType());

            var statusCodeResult = (StatusCodeResult)result;
            Assert.Equal((int)HttpStatusCode.PreconditionFailed, statusCodeResult.StatusCode);
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
            _mockUserRepository.Setup(r => r.Get(1)).ReturnsAsync(user).Verifiable();
            _mockUserRepository.Setup(r => r.Delete(1)).Returns(Task.Delay(0)).Verifiable();
            SetupController();

            //Act
            var result = await _controller.Delete(1);

            //Assert
            Assert.Equal(typeof(OkResult), result.GetType());

            var okResult = (OkResult)result;
            Assert.Equal(200, okResult.StatusCode);

            _mockUserRepository.Verify(r => r.Get(1), Times.Once);
            _mockUserRepository.Verify(r => r.Delete(1), Times.Once);
        }

        [Fact]
        [Trait("Category", "User Controller Tests")]
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

            _mockUserRepository.Verify(r => r.Get(2), Times.Once);
        }

        [Fact]
        [Trait("Category", "User Controller Tests")]
        public async Task Delete_invalid_etag_returns_precondition_failed()
        {
            //Arrange
            _mockMapper.Setup(am => am.Map(It.IsAny<UserModel>(), It.IsAny<User>())).Verifiable();

            var existingUser = new User
            {
                RowVersion = Encoding.ASCII.GetBytes("A Different RowVersion")
            };
            _mockUserRepository.Setup(ur => ur.Get(1)).ReturnsAsync(existingUser).Verifiable();

            SetupController();

            //Act
            var result = await _controller.Delete(1);

            //Assert
            Assert.Equal(typeof(StatusCodeResult), result.GetType());

            var statusCodeResult = (StatusCodeResult)result;
            Assert.Equal((int)HttpStatusCode.PreconditionFailed, statusCodeResult.StatusCode);
        }
    }
}
