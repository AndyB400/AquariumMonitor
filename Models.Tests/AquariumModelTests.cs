using System;
using AquariumMonitor.Models.ViewModels;
using Xunit;

namespace Models.Tests
{
    public class AquariumModelTests : TestBase
    {
        public AquariumModelTests()
        {
            item = new AquariumModel();
        }

        [Fact]
        public void TestName_Required()
        {
            CheckNotNullResult(item, "Name is required");
        }

        [Fact]
        public void TestType_Required()
        {
            CheckNotNullResult(item, "Type Name is required");
        }
    }
}
