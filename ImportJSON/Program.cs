using EliteJsonApi.Data;
using EliteJsonApi.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ImportJsonAndCsv
{
    class Program
    {
        static void Main(string[] args)
        {
            // Takes my ingredients file
            ImportIngredients("C:\\ingredients.json");

            // Takes EDDB's populated systems JSON dump
            ImportPopSystemsJson("C:\\systems_populated.json", false);

            // Takes EDDB's factions JSON
            ImportFactionsJson("C:\\factions.json");

            // Takes EDDB's populated system JSON file again to update controlling factions and faction presences
            ImportPopSystemsJson("C:\\systems_populated.json", true);

            // Takes EDDB's systems CSV (for unpopulated systems)
            //ImportSystemsCsv("C:\\systems.csv");

            // Takes EDSM's Celetial Bodies nightly JSON dump
            ImportEdsmBodiesJson("C:\\edsm_bodies.json");
        }

        private static void ImportIngredients(string path)
        {
            var root = JObject.Parse(File.ReadAllText(path));

            foreach (JProperty j in root.Children())
            {
                TryAddOrUpdateIngredient(new Material
                {
                    Name = j.Name,
                    Grade = j.Value.Value<string>("grade"),
                    Type = j.Value.Value<string>("type")
                });
            }
        }

        private static void TryAddOrUpdateIngredient(Material material)
        {
            DbContextOptionsBuilder<EliteDbContext> config = new DbContextOptionsBuilder<EliteDbContext>();
            config.UseNpgsql("Server=localhost;Port=5432;Database=EliteDB;UserID=default;Password=123456");

            using (EliteDbContext context = new EliteDbContext(config.Options))
            {
                if (context.Material.Count(m => m.Name.Equals(material.Name)) == 0)
                {
                    context.Material.Add(material);
                    context.SaveChanges();
                    Console.WriteLine("Added material '" + material.Name + "'");
                }
            }
        }

        private static void ImportFactionsJson(string v)
        {
            var array = JArray.Parse(File.ReadAllText(v));
            Console.WriteLine();

            foreach (JObject jo in array)
            {
                try
                {
                    var mf = new MinorFaction
                    {
                        Id = jo.Value<long>("id"),
                        Name = jo.Value<string>("name"),
                        UpdatedAt = DateTimeOffset.FromUnixTimeSeconds(jo.Value<long>("updated_at")).UtcDateTime,
                        Allegiance = jo.Value<string>("allegiance"),
                        Government = jo.Value<string>("government"),
                        State = jo.Value<string>("state"),
                        HomeSystemId = jo.Value<long>("home_system_id"),
                        IsPlayerFaction = jo.Value<bool>("is_player_faction")
                    };

                    TryAddOrUpdateFaction(mf);
                }
                catch
                {

                }
            }
        }

        private static void ImportPopSystemsJson(string path, bool parseFactions)
        {
            var array = JArray.Parse(File.ReadAllText(path));
            int errCount = 0, errCount2 = 0;

            foreach (JObject jo in array)
            {
                try
                {
                    var ss = new StarSystem
                    {
                        Allegiance = jo.Value<string>("allegiance"),
                        EddbId = jo.Value<long?>("id"),
                        EdsmId = jo.Value<long?>("edsm_id"),
                        Government = jo.Value<string>("government"),
                        IsPopulated = true,
                        State = jo.Value<string>("state"),
                        PrimaryEconomy = jo.Value<string>("primary_economy"),
                        Name = jo.Value<string>("name"),
                        Security = jo.Value<string>("security"),
                        PowerPlayLeader = jo.Value<string>("power"),
                        PowerPlayState = jo.Value<string>("power_state"),
                        Population = jo.Value<long>("population"),
                        X = jo.Value<double>("x"),
                        Y = jo.Value<double>("y"),
                        Z = jo.Value<double>("z"),
                        NeedsPermit = jo.Value<bool>("needs_permit"),
                        SimbadRef = jo.Value<string>("simbad_ref"),
                        UpdatedAt = DateTimeOffset.FromUnixTimeSeconds(jo.Value<long>("updated_at")).UtcDateTime
                    };

                    if (parseFactions)
                    {
                        ss.ControllingMinorFactionId = jo.Value<long>("controlling_minor_faction_id");

                        var presences = jo.Value<JArray>("minor_faction_presences");

                        foreach (JObject m in presences)
                        {
                            try
                            {
                                var mfp = new MinorFactionPresence
                                {
                                    MinorFactionId = m.Value<long>("minor_faction_id"),
                                    Influence = m.Value<double>("influence")
                                };

                                TryAddOrUpdateFactionPresence(mfp, ss);
                            }
                            catch (DbUpdateException)
                            {
                                errCount++;
                            }
                        }
                    }

                    TryAddOrUpdateSystem(ss);
                }
                catch (NullReferenceException)
                {
                    //Console.WriteLine(jo);
                }
                catch (InvalidCastException)
                {
                    Console.WriteLine("***" + jo.Value<string>("name"));
                }
                catch (DbUpdateException)
                {
                    errCount2++;
                }
            }

            Console.WriteLine("Presence errors: " + errCount);
            Console.WriteLine("Controlling errors: " + errCount);
        }

        private static void ImportSystemsCsv(string path)
        {
            using (StreamReader sr = File.OpenText(path))
            {
                bool isHeader = true;
                Dictionary<string, int> rv = new Dictionary<string, int>();
                string line;
                long lineNum = 0;
                while ((line = sr.ReadLine()) != null)
                {
                    if (isHeader)
                    {
                        int i = 0;
                        foreach (string s in line.Split(','))
                        {
                            rv.Add(s, i++);
                        }
                        isHeader = false;
                        //while (++lineNum < 4770000) sr.ReadLine();
                        Console.WriteLine("Start");
                    }
                    else
                    {
                        try
                        {
                            line = line.Replace("\"", "");
                            string[] record = line.Split(',');
                            if (record.Length <= 28)// && bool.TryParse(record[rv["is_populated"]], out bool b))
                            {
                                StarSystem ss = new StarSystem
                                {
                                    EddbId = long.Parse(record[rv["id"]]),
                                    EdsmId = long.Parse(record[rv["edsm_id"]]),
                                    Name = record[rv["name"]],
                                    SimbadRef = record[rv["simbad_ref"]],
                                    X = double.Parse(record[rv["x"]]),
                                    Y = double.Parse(record[rv["y"]]),
                                    Z = double.Parse(record[rv["z"]]),
                                    IsPopulated = bool.TryParse(record[rv["is_populated"]], out bool b) ? b : false,
                                    Population = long.TryParse(record[rv["population"]], out long l) ? l : 0,
                                    NeedsPermit = bool.TryParse(record[rv["needs_permit"]], out b) ? b : false,
                                    UpdatedAt = new DateTime(long.TryParse(record[rv["updated_at"]], out l) ? l : 0),
                                    Allegiance = record[rv["allegiance"]].Length == 0 ? null : record[rv["allegiance"]],
                                    Government = record[rv["government"]].Length == 0 ? null : record[rv["government"]],
                                    State = record[rv["state"]].Length == 0 ? null : record[rv["state"]],
                                    PrimaryEconomy = record[rv["primary_economy"]].Length == 0 ? null : record[rv["primary_economy"]],
                                    PowerPlayLeader = record[rv["power"]].Length == 0 ? null : record[rv["power"]],
                                    PowerPlayState = record[rv["power_state"]].Length == 0 ? null : record[rv["power_state"]],
                                    Security = record[rv["security"]].Length == 0 ? null : record[rv["security"]],
                                    Reserves = record[rv["reserve_type"]].Length == 0 ? null : record[rv["reserve_type"]],
                                    //ControllingMinorFactionId = long.TryParse(record[rv["controlling_minor_faction_id"]], out l) ? l : default(long?)
                                };

                                // For now, only save uninhabited systems up to 2500 LY from Sol
                                if (Math.Abs(ss.X) < 500 && Math.Abs(ss.Y) < 500 && Math.Abs(ss.Z) < 500)
                                {
                                    TryAddOrUpdateSystem(ss, false); // Don't update these systems (don't want to screw existing Pop. System data)
                                }
                            }
                            //if (++lineNum % 100000 == 0) ; //Console.WriteLine(lineNum);
                            //if (line.Contains(",true,")) Console.WriteLine(lineNum + " - " + line);
                        }
                        catch (ArgumentNullException) { }
                        catch (NullReferenceException) { }
                        catch (FormatException) { }
                    }
                }
            }
        }

        private static void ImportEdsmBodiesJson(string path)
        {
            // EDSM document is in a similar format to Newline-Delimited JSON, so streaming is the way to go
            using (StreamReader sr = File.OpenText(path))
            {
                string line = sr.ReadLine(); // Skip the opening array token
                while ((line = sr.ReadLine()) != null)
                {
                    line = line.TrimEnd(','); // Remove the ending comma to ensure valid JSON
                    JObject jo = JObject.Parse(line);

                    Body body;
                    bool added = false;

                    if (jo.Value<string>("type").ToLower().Equals("star"))
                    {
                        body = new Star();
                        body = new Star
                        {
                            Id = jo.Value<long>("id"),
                            Type = jo.Value<string>("type"),
                            EdsmId = jo.Value<long?>("id"),
                            StarSystemId = jo.Value<long>("systemId"), // Give it the EDSM ID, switch to inserts and updates
                            Name = jo.Value<string>("name"),
                            SubType = jo.Value<string>("subType"),
                            DistanceToArrival = jo.Value<double>("distanceToArrival"),
                            SolarMasses = jo.Value<float?>("solarMasses"),
                            SolarRadius = jo.Value<double?>("solarRadius"),
                            SurfaceTemperature = jo.Value<int>("surfaceTemperature"),
                            OrbitalPeriod = jo.Value<double?>("orbitalPeriod"),
                            SemiMajorAxis = jo.Value<double?>("semiMajorAxis"),
                            OrbitalEccentricity = jo.Value<double?>("orbitalEccentricity"),
                            OrbitalInclination = jo.Value<float?>("orbitalInclination"),
                            ArgOfPeriapsis = jo.Value<double?>("argOfPeriapsis"),
                            RotationalPeriod = jo.Value<double?>("rotationalPeriod"),
                            IsTidallyLocked = jo.Value<bool>("rotationalPeriodTidallyLocked"),
                            AxialTilt = jo.Value<double?>("axialTilt"),
                            AbsoluteMagnitude = jo.Value<float?>("absoluteMagnitude"),
                            Age = jo.Value<uint?>("age"),
                            IsMainStar = jo.Value<bool>("isMainStar"),
                            IsScoopable = jo.Value<bool>("isScoopable"),
                            LuminosityClass = jo.Value<string>("luminosity")
                        };

                        if (body.SubType.Contains("lack"))
                            (body as Star).SpectralClass = "BH";
                        else if (body.SubType.Contains("eutron"))
                            (body as Star).SpectralClass = "N";
                        else if (body.SubType[1].Equals(' '))
                            (body as Star).SpectralClass = body.SubType.Substring(0, 1);
                    }
                    else //if (jo.Value<string>("type").ToLower().Equals("planet"))
                    {
                        body = new Planet
                        {
                            Id = jo.Value<long>("id"),
                            Type = jo.Value<string>("type"),
                            EdsmId = jo.Value<long?>("id"),
                            StarSystemId = jo.Value<long>("systemId"), // Give it the EDSM ID, switch to inserts and updates
                            Name = jo.Value<string>("name"),
                            SubType = jo.Value<string>("subType"),
                            IsLandable = jo.Value<bool>("isLandable"),
                            DistanceToArrival = jo.Value<double>("distanceToArrival"),
                            Gravity = jo.Value<double?>("gravity"),
                            EarthMasses = jo.Value<double?>("earthMasses"),
                            Radius = jo.Value<float?>("radius"),
                            SurfaceTemperature = jo.Value<int>("surfaceTemperature"),
                            VolcanismType = jo.Value<string>("volcanismType"),
                            AtmosphereType = jo.Value<string>("atmosphereType"),
                            TerraformingState = jo.Value<string>("terraformingState"),
                            OrbitalPeriod = jo.Value<double?>("orbitalPeriod"),
                            SemiMajorAxis = jo.Value<double?>("semiMajorAxis"),
                            OrbitalEccentricity = jo.Value<double?>("orbitalEccentricity"),
                            OrbitalInclination = jo.Value<float?>("orbitalInclination"),
                            ArgOfPeriapsis = jo.Value<double?>("argOfPeriapsis"),
                            RotationalPeriod = jo.Value<double?>("rotationalPeriod"),
                            IsTidallyLocked = jo.Value<bool>("rotationalPeriodTidallyLocked"),
                            AxialTilt = jo.Value<double?>("axialTilt")
                        };

                        added = TryAddOrUpdateBody(body);
                    }
                    if (added && jo.TryGetValue("materials", out JToken mats))
                    {
                        foreach (JProperty jp in mats)
                        {
                            var mat = new RawMaterialShare
                            {
                                BodyId = body.Id,
                                Share = float.Parse(jp.Value.Value<string>())
                            };

                            TryAddOrUpdateMaterialShare(mat, jp.Name);
                        }
                    }

                    if (added && jo.TryGetValue("atmosphereComposition", out JToken atm))
                    {
                        foreach (JProperty jp in atm)
                        {
                            var ac = new AtmosphereComposite
                            {
                                BodyId = body.Id,
                                Component = jp.Name,
                                Share = float.TryParse(jp.Value.Value<string>(), out float f) ? f : default(float?)
                            };

                            TryAddOrUpdateAtmosComp(ac);
                        }
                    }

                    if (added && jo.TryGetValue("rings", out JToken rings))
                    {
                        foreach (JObject jp in rings)
                        {
                            var ring = new Ring
                            {
                                BodyId = body.Id,
                                Name = jp.Value<string>("name"),
                                Type = jp.Value<string>("type"),
                                Mass = long.TryParse(jp.Value<string>("mass"), out long l) ? l : default(long?),
                                InnerRadius = jp.Value<int?>("innerRadius"),
                                OuterRadius = jp.Value<int?>("outerRadius")
                            };

                            TryAddOrUpdateRing(ring);
                        }
                    }
                }
            }
        }

        private static void TryAddOrUpdateRing(Ring ring)
        {
            DbContextOptionsBuilder<EliteDbContext> config = new DbContextOptionsBuilder<EliteDbContext>();
            config.UseNpgsql("Server=localhost;Port=5432;Database=EliteDB;UserID=default;Password=123456");

            using (EliteDbContext context = new EliteDbContext(config.Options))
            {
                if (context.Ring.Count(r => r.Name == ring.Name && r.BodyId == ring.BodyId) == 0)
                {
                    context.Ring.Add(ring);
                    context.SaveChanges();
                }
                else
                {
                    context.Ring.Update(ring);
                    context.SaveChanges();
                }
            }
        }

        private static void TryAddOrUpdateAtmosComp(AtmosphereComposite ac)
        {
            DbContextOptionsBuilder<EliteDbContext> config = new DbContextOptionsBuilder<EliteDbContext>();
            config.UseNpgsql("Server=localhost;Port=5432;Database=EliteDB;UserID=default;Password=123456");

            using (EliteDbContext context = new EliteDbContext(config.Options))
            {
                if (context.AtmosphereComposite.Count(r => r.Component == ac.Component && r.BodyId == ac.BodyId) == 0)
                {
                    context.AtmosphereComposite.Add(ac);
                    context.SaveChanges();
                }
                else
                {
                    context.AtmosphereComposite.Update(ac);
                    context.SaveChanges();
                }
            }
        }

        private static void TryAddOrUpdateMaterialShare(RawMaterialShare mat, string name)
        {
            DbContextOptionsBuilder<EliteDbContext> config = new DbContextOptionsBuilder<EliteDbContext>();
            config.UseNpgsql("Server=localhost;Port=5432;Database=EliteDB;UserID=default;Password=123456");

            using (EliteDbContext context = new EliteDbContext(config.Options))
            {
                mat.MaterialId = context.Material.Where(m => string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase)).Single().Id;
                if (context.RawMaterialShare.Count(r => r.MaterialId == mat.MaterialId && r.BodyId == mat.BodyId) == 0)
                {
                    context.RawMaterialShare.Add(mat);
                    context.SaveChanges();
                }
                else
                {
                    context.RawMaterialShare.Update(mat);
                    context.SaveChanges();
                }
            }
        }

        private static void TryAddOrUpdateSystem(StarSystem system, bool shouldUpdate = true)
        {
            DbContextOptionsBuilder<EliteDbContext> config = new DbContextOptionsBuilder<EliteDbContext>();
            config.UseNpgsql("Server=localhost;Port=5432;Database=EliteDB;UserID=default;Password=123456");

            using (EliteDbContext context = new EliteDbContext(config.Options))
            {
                long i = context.StarSystem.Where(ss => ss.Name == system.Name).Select(ss => ss.Id).FirstOrDefault();
                if (i == 0)// && system.IsPopulated)
                {
                    context.StarSystem.Add(system);
                    context.SaveChanges();
                    Console.WriteLine($"Added '{system.Name}'");
                }
                if (i == 1 && shouldUpdate)
                {
                    system.Id = i;
                    context.StarSystem.Update(system);
                    context.SaveChanges();
                    Console.WriteLine($"Updated '{system.Name}'");
                }
            }
        }

        private static void TryAddOrUpdateFactionPresence(MinorFactionPresence mfp, StarSystem ss)
        {
            DbContextOptionsBuilder<EliteDbContext> config = new DbContextOptionsBuilder<EliteDbContext>();
            config.UseNpgsql("Server=localhost;Port=5432;Database=EliteDB;UserID=default;Password=123456");

            using (EliteDbContext context = new EliteDbContext(config.Options))
            {
                mfp.StarSystemId = context.StarSystem.Where(s => s.EddbId == ss.EddbId).Select(s => s.Id).FirstOrDefault();
                if (mfp.StarSystemId == 0) return;

                if (context.MinorFactionPresence.Where(m => m.StarSystemId == mfp.StarSystemId && m.MinorFactionId == mfp.MinorFactionId).Count() == 0)
                {
                    Console.WriteLine("*");
                    context.MinorFactionPresence.Add(mfp);
                    context.SaveChanges();
                }
                else
                {
                    context.MinorFactionPresence.Update(mfp);
                    context.SaveChanges();
                }
            }
        }

        private static void TryAddOrUpdateFaction(MinorFaction faction)
        {
            DbContextOptionsBuilder<EliteDbContext> config = new DbContextOptionsBuilder<EliteDbContext>();
            config.UseNpgsql("Server=localhost;Port=5432;Database=EliteDB;UserID=default;Password=123456");

            using (EliteDbContext context = new EliteDbContext(config.Options))
            {
                long i = context.MinorFaction.Where(f => f.Name == faction.Name).Select(f => f.Id).FirstOrDefault();
                long hsid = context.StarSystem.Where(ss => ss.EddbId == faction.HomeSystemId).Select(ss => ss.Id).FirstOrDefault();
                faction.HomeSystemId = hsid;

                if (i == 0)// && system.IsPopulated)
                {
                    context.MinorFaction.Add(faction);
                    context.SaveChanges();
                    Console.WriteLine($"Added '{faction.Name}'");
                }
                else// if (i == 1)
                {
                    context.MinorFaction.Update(faction);
                    context.SaveChanges();
                    Console.WriteLine($"Updated '{faction.Name}'");
                }
            }
        }

        private static bool TryAddOrUpdateBody(Body body)
        {
            DbContextOptionsBuilder<EliteDbContext> config = new DbContextOptionsBuilder<EliteDbContext>();
            config.UseNpgsql("Server=localhost;Port=5432;Database=EliteDB;UserID=default;Password=123456");

            using (EliteDbContext context = new EliteDbContext(config.Options))
            {
                try
                {
                    StarSystem system = context.StarSystem.Where(ss => ss.EdsmId == body.StarSystemId).FirstOrDefault();
                    body.StarSystemId = system.Id;

                    Body temp = context.Body.Where(b => b.EdsmId == body.EdsmId).FirstOrDefault();

                    if (temp != null)
                    {
                        //body.Id = temp.Id;
                        //context.Body.Update(body);
                        //context.SaveChanges();
                        //return true;
                    }
                    else // for now, only add populated system bodies or bodies within 300 LY
                    {
                        if (system.IsPopulated || system.DistanceTo(new StarSystem { X = 0, Y = 0, Z = 0 }) <= 250)
                        {
                            context.Body.Add(body);
                            context.SaveChanges();
                            Console.WriteLine(body.Name);
                            return true;
                        }
                    }

                    return false;
                }
                catch (NullReferenceException)
                {
                    return false;
                }
            }
        }
    }
}
