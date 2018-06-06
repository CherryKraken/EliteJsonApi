using EliteJsonApi.Models;
using System;
using Xunit;

namespace EliteJsonApi.Tests
{
    public class ModelAndOptionsTests
    {
        private StarSystem _starSystem;

        public ModelAndOptionsTests()
        {
            _starSystem = new StarSystem { X = 0, Y = 0, Z = 0 };
        }

        [Fact]
        public void TestStarSystemDistanceTo()
        {
            Assert.Equal(0, _starSystem.DistanceTo(_starSystem));
            Assert.Equal(129, _starSystem.DistanceTo(new StarSystem { X = -65.22, Y = 7.75, Z = -111.03 }), 2);
            Assert.Throws<ArgumentNullException>(() => _starSystem.DistanceTo(null));
        }

        [Fact]
        public void TestOptionsAttributes()
        {

        }
    }
}
