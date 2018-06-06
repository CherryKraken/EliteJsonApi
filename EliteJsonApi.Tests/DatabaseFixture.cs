using EliteJsonApi.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace EliteJsonApi.Tests
{
    public class DatabaseFixture : IDisposable
    {
        public EliteDbContext Context;

        public void Dispose()
        {

        }
    }
}
