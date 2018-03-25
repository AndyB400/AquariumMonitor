using AquariumMonitor.Models;
using System.Collections.Generic;
using Xunit;

namespace AquariumMonitor.BusinessLogic.Tests
{
    public class MeasurementManagerTests
    {
        private readonly MeasurementManager _measurementManager;

        public MeasurementManagerTests()
        {
            _measurementManager = new MeasurementManager();
        }

        public static IEnumerable<object[]> GetMeasurementTypes()
        {
            yield return new object[] { new MeasurementType { Id = 0, Name = "NH4" }, 1 };
            yield return new object[] { new MeasurementType { Id = 0, Name = "NO2" }, 2 };
            yield return new object[] { new MeasurementType { Id = 0, Name = "PH" }, 3 };
            yield return new object[] { new MeasurementType { Id = 0, Name = "Temp" }, 4 };
        }

        [Theory]
        [MemberData(nameof(GetMeasurementTypes))]
        public void TestLookupFromName(MeasurementType type, int expectedId)
        {
            // Arrange

            // Act
            type = _measurementManager.LookupFromName(type);

            // Assert
            Assert.Equal(expectedId, type.Id);
        }
    }
}
