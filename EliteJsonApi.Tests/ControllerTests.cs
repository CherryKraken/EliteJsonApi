using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace EliteJsonApi.Tests
{
    public class ControllerTests : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _fixture;

        internal ControllerTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }
    }
}
