using System;
using AquariumMonitor.APIModels;
using Xunit;

namespace Models.Tests
{
    public class WaterChangeModelTests : TestBase
    {
        public WaterChangeModelTests()
        {
            item = new WaterChangeModel();
        }

        [Fact]
        public void TestPercentageChanged_UpperRange()
        {
            (item as WaterChangeModel).PercentageChanged = 101;
            CheckNotNullResult(item, "PercentageChanged must be between 1 and 100");
        }

        [Fact]
        public void TestPercentageChanged_LowerRange()
        {
            (item as WaterChangeModel).PercentageChanged = 0;
            CheckNotNullResult(item, "PercentageChanged must be between 1 and 100");
        }

        [Fact]
        public void TestChanged_UpperRange()
        {
            (item as WaterChangeModel).Changed = new DateTime(2032, 1, 2);
            CheckNotNullResult(item, "Changed value of '02-Jan-2032' is invalid. Must be between 01-Jan-2016 and 01-Jan-2032");
        }

        [Fact]
        public void TestChanged_LowerRange()
        {
            (item as WaterChangeModel).Changed = new DateTime(2015, 12, 31);
            CheckNotNullResult(item, "Changed value of '31-Dec-2015' is invalid. Must be between 01-Jan-2016 and 01-Jan-2032");
        }
    }
}
