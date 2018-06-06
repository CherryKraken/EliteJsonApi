using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace EliteJsonApi.Tests
{
    public class ControllerTests : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _fixture;

        public ControllerTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void TestSystemsInDB()
        {
            Assert.Equal(2, _fixture.Context.StarSystem.Count());
        }
    }
}
