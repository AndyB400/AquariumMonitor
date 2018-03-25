using AquariumMonitor.BusinessLogic;
using AquariumMonitor.DAL.Interfaces;
using AquariumMonitor.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using Xunit;

namespace BusinessLogic.Tests
{
    public class AuthManagerTests
    {
        private readonly AuthManager authManager;
        Mock iConfigurationMock;
        Mock iUserRepositoryMock;

        public AuthManagerTests()
        {
            iConfigurationMock = new Mock<IConfiguration>();
            iUserRepositoryMock = new Mock<IUserRepository>();

            //var itemMock = new Mock<IMyObject>();
            //var items = new List<IMyObject> { itemMock.Object }; //<--IEnumerable<IMyObject>

            //var mock = new Mock<IMyCollection>();
            //mock.Setup(m => m.Count).Returns(() => items.Count);
            //mock.Setup(m => m[It.IsAny<int>()]).Returns<int>(i => items.ElementAt(i));
            //mock.Setup(m => m.GetEnumerator()).Returns(() => items.GetEnumerator());


            authManager = new AuthManager((IConfiguration)iConfigurationMock.Object, (IUserRepository)iUserRepositoryMock.Object);
        }

        [Fact]
        [Trait("Category", "Auth Manager")]
        public void TestUserClaims()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                FirstName = "Fish",
                LastName = "Keeper",
                Email = "fish.keeper@aquariumMonitor.com"
            };

            // Act
            var claims = authManager.CreateUserClaims(user);

            //Assert
            Assert.Equal("1", claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub.ToString()).Value);
            Assert.Equal("Fish", claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.GivenName.ToString()).Value);
            Assert.Equal("Keeper", claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.FamilyName.ToString()).Value);
            Assert.Equal("fish.keeper@aquariumMonitor.com", claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email.ToString()).Value);
        }

        [Fact]
        [Trait("Category", "Auth Manager")]
        public void TestCreateToken()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                FirstName = "Fish",
                LastName = "Keeper",
                Email = "fish.keeper@aquariumMonitor.com"
            };

            var claims = authManager.CreateUserClaims(user);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("abc123"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Act
            var token = authManager.CreateToken(claims, 15, creds);

     
        }
    }
}
