using System;
using System.Collections.Generic;
using System.Data;
using AquariumMonitor.DAL.Interfaces;
using AquariumMonitor.Models;
using Moq;
using Dapper;
using Xunit;

namespace DAL.Tests
{
    public class AquariumRepositoryTests
    {
        private Mock<IConnectionFactory> mockConnectionFactory;
        private IAquariumRepository aquariumRepository;

        public AquariumRepositoryTests()
        {
            var aquarium = new Aquarium
            {
                Name = "Lounge",
                Created = new DateTimeOffset(2018, 01, 02, 14, 00, 00, TimeSpan.MinValue),
                Height = 45,
                Length = 37,
                Width = 98,
                ActualVolume = 172,
                OfficalVolume = 200
            };

            var mockConnection = new Mock<IDbConnection>();
            mockConnection.SetupProperty(m => m.ConnectionString);
           // mockConnection.Setup(m => m.QueryFirstOrDefault(string.Empty)).Returns(aquarium);

            //mockConnectionFactory = new Mock<IConnectionFactory>();
            //mockConnectionFactory.Setup(cf => cf.GetOpenConnection()).Returns(mockConnection);

            //var aquariums = new List<IAquarium>();
            //aquariums.Add(new Aquarium
            //{
            //    Name = "Lounge",
            //    Created = new DateTimeOffset(2018, 01, 02, 14, 00, 00, TimeSpan.MinValue),
            //    Height = 45,
            //    Length = 37,
            //    Width = 98,
            //    ActualVolume = 172,
            //    OfficalVolume = 200
            //});

            //mockRepository = new Mock<IAquariumRepository>();
            //mockRepository.Setup(m => m.GetForUser(1)).Returns(aquariums);
        }

        //[Fact]
        //public void TestGetForUser()
        //{
            //Arrange
            

            //Act
           //var aquariums = mockRepository.Object.GetForUser(1);

           // //Assert
           // Assert.AreEqual(1, aquariums.Count);
        //}
    }
}
