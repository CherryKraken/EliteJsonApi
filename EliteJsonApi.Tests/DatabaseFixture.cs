using EliteJsonApi.Data;
using EliteJsonApi.Models;
using EliteJsonApi.Models.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EliteJsonApi.Tests
{
    public class DatabaseFixture
    {
        public EliteDbContext Context { get; }

        public DatabaseFixture()
        {
            var config = new DbContextOptionsBuilder<EliteDbContext>();
            config.UseInMemoryDatabase("tempdb");
            Context = new EliteDbContext(config.Options);

            Context.StarSystem.AddRange(TestSystems);
            Context.SaveChanges();
        }

        public readonly IReadOnlyList<StarSystem> TestSystems = new List<StarSystem>
        {
            new StarSystem
            {
                Name = "Sol",
                X = 0, Y = 0, Z = 0,
                IsPopulated = true,
                Population = 30000000000,
                Allegiance = "Federation",
                PrimaryEconomy = "Service",
                Security = "High",
                State = "Boom",
                PowerPlayLeader = "Zachary Hudson"
            },
            new StarSystem
            {
                Name = "Sagittarius A*",
                X = 20, Y = 24670, Z = -1012,
                IsPopulated = false
            }
        };
    }
}
