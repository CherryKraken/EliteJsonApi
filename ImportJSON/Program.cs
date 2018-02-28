using EliteJsonApi.Data;
using EliteJsonApi.Models;
using EliteJsonApi.Models.Helpers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ImportJsonAndCsv
{
    class Program
    {
        private const string _connectionString = "Server=localhost;Port=5432;Database=EliteDB;UserID=postgres;Password=password";
        private static string _dir;

        static void Main(string[] args)
        {
            if (args.Length > 0 && Directory.Exists(args[0]))
            {
                _dir = args[0];

                // Make debug data directory if it does not exist
                if (!Directory.Exists(_dir + "/debug/"))
                {
                    Directory.CreateDirectory(_dir + "/debug/");
                }

                // Takes my ingredients file
                //ImportIngredients($"{dir}/ingredients.json");

                // Takes EDDB's populated systems JSON dump
                //ImportPopSystemsJson($"{dir}/systems_populated.json", false);

                // Takes EDDB's factions JSON
                //ImportFactionsJson($"{dir}/factions.json");

                // Takes EDDB's populated system JSON file again to update controlling factions and faction presences
                ImportPopSystemsJson($"{_dir}/systems_populated.json", true);

                // Takes EDDB's systems CSV (for unpopulated systems)
                //ImportSystemsCsv($"{dir}/systems.csv");

                // Update systems with NameLower
                //SetSystemNameLower();

                // Takes EDSM's Celetial Bodies nightly JSON dump
                //ImportEdsmBodiesJson($"{dir}/edsm_bodies.json");
            }
            else
            {
                Console.WriteLine("Please pass a valid path to the data directory\n\te.g. dotnet run -- /home/user/data/");
            }
        }

        private static void SetSystemNameLower()
        {
            DbContextOptionsBuilder<EliteDbContext> config = new DbContextOptionsBuilder<EliteDbContext>();
            config.UseNpgsql(_connectionString);

            using (EliteDbContext context = new EliteDbContext(config.Options))
            {
                foreach (StarSystem ss in context.StarSystem)
                {
                    ss.NameLower = ss.Name.ToLower();
                    context.Update(ss);
                }
                context.SaveChanges();
            }
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
                    Type = j.Value.Value<string>("type"),
                    Method = j.Value.Value<string>("method"),
                    MethodDescription = j.Value.Value<string>("methodDesc")
                });
            }
        }

        private static void TryAddOrUpdateIngredient(Material material)
        {
            DbContextOptionsBuilder<EliteDbContext> config = new DbContextOptionsBuilder<EliteDbContext>();
            config.UseNpgsql(_connectionString);

            using (EliteDbContext context = new EliteDbContext(config.Options))
            {
                var temp = context.Material.Select(m => m.Name.Equals(material.Name)).Count();
                if (temp == 0)
                {
                    context.Material.Add(material);
                    context.SaveChanges();
                    Console.WriteLine("Added material '" + material.Name + "'");
                }
                else
                {
                    material.Id = context.Material.Where(m => m.Name.Equals(material.Name)).Select(m => m.Id).First();
                    context.Material.Update(material);
                    context.SaveChanges();
                    Console.WriteLine("Updated material '" + material.Name + "'");
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
                        Allegiance = jo.Value<string>("allegiance").ToNormalCase(LookupOptions.Allegiances),
                        Government = jo.Value<string>("government").ToNormalCase(LookupOptions.Governments),
                        State = jo.Value<string>("state").ToNormalCase(LookupOptions.States),
                        HomeSystemId = jo.Value<long>("home_system_id"),
                        IsPlayerFaction = jo.Value<bool>("is_player_faction")
                    };

                    TryAddOrUpdateFaction(mf);
                }
                catch
                {
                    File.AppendAllText($"{_dir}/debug/debug.txt", $"faction,{jo.Value<string>("name")},{jo.Value<long?>("id")}\n");
                }
            }
        }

        private static void ImportPopSystemsJson(string path, bool parseFactions)
        {
            var array = JArray.Parse(File.ReadAllText(path));

            foreach (JObject jo in array)
            {
                try
                {
                    var ss = new StarSystem
                    {
                        Allegiance = jo.Value<string>("allegiance").ToNormalCase(LookupOptions.Allegiances),
                        EddbId = jo.Value<long?>("id"),
                        EdsmId = jo.Value<long?>("edsm_id"),
                        Government = jo.Value<string>("government").ToNormalCase(LookupOptions.Governments),
                        IsPopulated = true,
                        State = jo.Value<string>("state").ToNormalCase(LookupOptions.States),
                        PrimaryEconomy = jo.Value<string>("primary_economy").ToNormalCase(LookupOptions.Economies),
                        Name = jo.Value<string>("name"),
                        NameLower = jo.Value<string>("name").ToLower(),
                        Security = jo.Value<string>("security").ToNormalCase(LookupOptions.SecurityTypes),
                        PowerPlayLeader = jo.Value<string>("power").ToNormalCase(LookupOptions.PowerPlayLeaders),
                        PowerPlayState = jo.Value<string>("power_state").ToNormalCase(LookupOptions.PowerEffects),
                        Reserves = jo.Value<string>("reserve_type").ToNormalCase(LookupOptions.ReserveTypes),
                        Population = jo.Value<long?>("population") ?? 0,
                        X = jo.Value<double>("x"),
                        Y = jo.Value<double>("y"),
                        Z = jo.Value<double>("z"),
                        NeedsPermit = jo.Value<bool?>("needs_permit") ?? false,
                        SimbadRef = jo.Value<string>("simbad_ref"),
                        UpdatedAt = DateTimeOffset.FromUnixTimeSeconds(jo.Value<long>("updated_at")).UtcDateTime
                    };

                    if (parseFactions)
                    {
                        ss.ControllingMinorFactionId = jo.Value<long?>("controlling_minor_faction_id");

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
                            catch
                            {
                                File.AppendAllText($"{_dir}/debug/debug.txt", $"eddb_mfp,{ss.Name},{m.Value<long?>("minor_faction_id")}\n");
                            }
                        }
                    }

                    TryAddOrUpdateSystem(ss);
                }
                catch
                {
                    File.AppendAllText($"{_dir}/debug/debug.txt", $"eddb_ss,{jo.Value<string>("name")},{jo.Value<long?>("id")}\n");
                }
            }
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

        /// <summary>
        /// EDSM's JSON is compressed, omits null-value properties
        /// </summary>
        /// <param name="path"></param>
        private static void ImportEdsmBodiesJson(string path)
        {
            // EDSM document is in a similar format to Newline-Delimited JSON, so streaming is the way to go
            using (StreamReader sr = File.OpenText(path))
            {
                string line = sr.ReadLine(); // Skip the opening array token

                // Get the current line
                long numLines = long.Parse(File.ReadAllText("E:\\bodies_exit_line.txt").Trim());

                // Skip to the current line to save time
                for (int i = 0; i < numLines; i++) sr.ReadLine();
                Console.WriteLine("Started parsing bodies");

                while ((line = sr.ReadLine()) != null)
                {
                    line = line.TrimEnd(','); // Remove the ending comma to ensure valid JSON
                    JObject jo = JObject.Parse(line);
                    var pKeys = jo.Properties().Select(p => p.Name);
                    bool added = false;

                    try
                    {
                        Body body = new Body
                        {
                            Id = jo.Value<long>("id"),
                            Type = jo.Value<string>("type").ToNormalCase(LookupOptions.BodyTypes),
                            EdsmId = jo.Value<long?>("id"),
                            StarSystemId = jo.Value<long>("systemId"), // Give it the EDSM ID, switch to my ID during inserts and updates
                            Name = jo.Value<string>("name")
                        };

                        if (pKeys.Contains("subType"))
                            body.SubType = jo.Value<string>("subType");

                        if (pKeys.Contains("distanceToArrival"))
                            body.DistanceToArrival = jo.Value<double?>("distanceToArrival");

                        if (pKeys.Contains("solarMasses"))
                            body.SolarMasses = jo.Value<float?>("solarMasses");

                        if (pKeys.Contains("solarRadius"))
                            body.SolarRadius = jo.Value<double?>("solarRadius");

                        if (pKeys.Contains("surfaceTemperature"))
                            body.SurfaceTemperature = jo.Value<int?>("surfaceTemperature");

                        if (pKeys.Contains("orbitalPeriod"))
                            body.OrbitalPeriod = jo.Value<double?>("orbitalPeriod");

                        if (pKeys.Contains("semiMajorAxis"))
                            body.SemiMajorAxis = jo.Value<double?>("semiMajorAxis");

                        if (pKeys.Contains("orbitalEccentricity"))
                            body.OrbitalEccentricity = jo.Value<double?>("orbitalEccentricity");

                        if (pKeys.Contains("orbitalInclination"))
                            body.OrbitalInclination = jo.Value<float?>("orbitalInclination");

                        if (pKeys.Contains("argOfPeriapsis"))
                            body.ArgOfPeriapsis = jo.Value<double?>("argOfPeriapsis");

                        if (pKeys.Contains("rotationalPeriod"))
                            body.RotationalPeriod = jo.Value<double?>("rotationalPeriod");

                        if (pKeys.Contains("rotationalPeriodTidallyLocked"))
                            body.IsTidallyLocked = jo.Value<bool?>("rotationalPeriodTidallyLocked");

                        if (pKeys.Contains("axialTilt"))
                            body.AxialTilt = jo.Value<double?>("axialTilt");

                        if (pKeys.Contains("absoluteMagnitude"))
                            body.AbsoluteMagnitude = jo.Value<float?>("absoluteMagnitude");

                        if (pKeys.Contains("age"))
                            body.Age = jo.Value<uint?>("age");

                        if (pKeys.Contains("isMainStar"))
                            body.IsMainStar = jo.Value<bool?>("isMainStar");

                        if (pKeys.Contains("isScoopable"))
                            body.IsScoopable = jo.Value<bool?>("isScoopable");

                        if (pKeys.Contains("luminosity"))
                            body.LuminosityClass = jo.Value<string>("luminosity");

                        // White Dwarf classes are 2-3 chars starting with D
                        if (body.Type.ToLower().Equals("star") && body.SubType.ToLower().Contains("white dwarf"))
                            body.SpectralClass = body.SubType.Split('(')[1].Split(')')[0];
                        // Herbig Stars are classed as Ae/Be
                        else if (body.Type.ToLower().Equals("star") && body.SubType.ToLower().Contains("herbig"))
                            body.SpectralClass = "Ae/Be";
                        // T Tauris / Proto-Stars are classed as T
                        else if (body.Type.ToLower().Equals("star") && body.SubType.ToLower().Contains("tauri"))
                            body.SpectralClass = "TT";
                        // Carbon Stars are classed as C or CN
                        else if (body.Type.ToLower().Equals("star") && body.SubType.ToLower().Equals("c star"))
                            body.SpectralClass = "C";
                        else if (body.Type.ToLower().Equals("star") && body.SubType.ToLower().Equals("cn star"))
                            body.SpectralClass = "CN";
                        // Set Black Holes and Neutron Stars to blank
                        else if (body.Type.ToLower().Equals("star") && (body.SubType.ToLower().Contains("black hole") || body.SubType.ToLower().Contains("neutron")))
                            body.SpectralClass = "";
                        // Set other stars to their letter class (e.g. M, F, O, etc.)
                        else if (body.Type.ToLower().Equals("star"))
                            body.SpectralClass = body.SubType.Split('(')[0];
                        // Rest are planets
                        else
                            body.SpectralClass = default(string);

                        if (pKeys.Contains("isLandable"))
                            body.IsLandable = jo.Value<bool?>("isLandable");

                        if (pKeys.Contains("gravity"))
                            body.Gravity = jo.Value<double?>("gravity");

                        if (pKeys.Contains("earthMasses"))
                            body.EarthMasses = jo.Value<double?>("earthMasses");

                        if (pKeys.Contains("radius"))
                            body.Radius = jo.Value<float?>("radius");

                        if (pKeys.Contains("volcanismType"))
                            body.VolcanismType = jo.Value<string>("volcanismType");

                        if (pKeys.Contains("atmosphereType"))
                            body.AtmosphereType = jo.Value<string>("atmosphereType");

                        if (pKeys.Contains("terraformingState"))
                            body.TerraformingState = jo.Value<string>("terraformingState");

                        // Don't even continue if this body wasn't added
                        added = TryAddOrUpdateBody(body);

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
                    catch
                    {
                        File.AppendAllText(@"E:\debug_bodies.txt", $"edsm_body,{jo.Value<string>("name")},{jo.Value<long?>("id")}\n");
                    }
                    File.WriteAllText("E:\\bodies_exit_line.txt", $"{numLines++}");
                }
            }
        }

        private static void TryAddOrUpdateRing(Ring ring)
        {
            DbContextOptionsBuilder<EliteDbContext> config = new DbContextOptionsBuilder<EliteDbContext>();
            config.UseNpgsql(_connectionString);

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
            config.UseNpgsql(_connectionString);

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
            config.UseNpgsql(_connectionString);

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
            config.UseNpgsql(_connectionString);

            using (EliteDbContext context = new EliteDbContext(config.Options))
            {
                long i = context.StarSystem.Where(ss => ss.Name == system.Name).Select(ss => ss.Id).FirstOrDefault();
                if (i == 0)// && system.IsPopulated)
                {
                    context.StarSystem.Add(system);
                    context.SaveChanges();
                    Console.WriteLine($"Added '{system.Name}'");
                }
                else if (shouldUpdate)
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
            config.UseNpgsql(_connectionString);

            using (EliteDbContext context = new EliteDbContext(config.Options))
            {
                mfp.StarSystemId = context.StarSystem.Where(s => s.EddbId == ss.EddbId).Select(s => s.Id).FirstOrDefault();
                if (mfp.StarSystemId == 0) return;

                if (context.MinorFactionPresence.Where(m => m.StarSystemId == mfp.StarSystemId && m.MinorFactionId == mfp.MinorFactionId).Count() == 0)
                {
                    context.MinorFactionPresence.Add(mfp);
                    context.SaveChanges();
                    Console.WriteLine("Added faction presence for system #" + mfp.StarSystemId);
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
            config.UseNpgsql(_connectionString);

            using (EliteDbContext context = new EliteDbContext(config.Options))
            {
                long i = context.MinorFaction.Where(f => f.Name == faction.Name).Select(f => f.Id).FirstOrDefault();
                long hsid = context.StarSystem.Where(ss => ss.EddbId == faction.HomeSystemId).Select(ss => ss.Id).FirstOrDefault();
                faction.HomeSystemId = hsid;

                if (i == 0 && hsid != 0)
                {
                    context.MinorFaction.Add(faction);
                    context.SaveChanges();
                    Console.WriteLine($"Added '{faction.Name}'");
                }
                else if (hsid != 0)
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
            config.UseNpgsql(_connectionString);

            using (EliteDbContext context = new EliteDbContext(config.Options))
            {
                try
                {
                    StarSystem system = context.StarSystem.Where(ss => ss.EdsmId == body.StarSystemId).FirstOrDefault();
                    body.StarSystemId = system.Id;

                    Body temp = context.Body.Where(b => b.EdsmId == body.EdsmId).FirstOrDefault();

                    if (system != null && temp != null)
                    {
                        body.Id = temp.Id;
                        context.Body.Update(body);
                        context.SaveChanges();
                        Console.WriteLine("Updated: " + body.Name);
                        return true;
                    }
                    else // for now, only add populated system bodies or bodies within 300 LY
                    {
                        if (system != null && (system.IsPopulated || system.DistanceTo(new StarSystem { X = 0, Y = 0, Z = 0 }) <= 250))
                        {
                            context.Body.Add(body);
                            context.SaveChanges();
                            Console.WriteLine(body.Name);
                            return true;
                        }
                    }

                    return false;
                }
                catch (DbUpdateException e)
                {
                    //Debugger.Break();
                    return false;
                }
                catch
                {
                    //File.AppendAllText(@"E:\debug_bodies.txt", $"edsm,{body.Name},{body.Id}\n");
                    return false;
                }
            }
        }
    }
}
