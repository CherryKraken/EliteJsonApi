using EliteJsonApi.Data;
using EliteJsonApi.Models;
using EliteJsonApi.Models.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EliteJsonApi.Controllers
{
    [Produces("application/json")]
    [Route("api/v1")]
    public class ApiController : Controller
    {
        private readonly EliteDbContext _context;

        public ApiController(EliteDbContext context)
        {
            _context = context;
        }

        // GET api/v1/ref
        [HttpGet("ref")]
        [Produces("text/plain")]
        public string Get()
        {
            return System.IO.File.ReadAllText("~/../endpoints.json");
            //dynamic data = JsonConvert.DeserializeObject(System.IO.File.ReadAllText("~/../endpoints.json"));
            //return JsonConvert.SerializeObject(data);
        }

        /// <summary>
        /// GET api/v1/bodies?[parameters]
        /// </summary>
        /// <returns><see cref="IEnumerable{Body}"/></returns>
        [HttpGet("bodies")]
        public IEnumerable<Body> GetBodies()
        {
            // Get and deserialize query from HTTP request
            Dictionary<string, StringValues> query = new Dictionary<string, StringValues>();
            foreach (KeyValuePair<string, StringValues> kv in Request.Query)
            {
                query.Add(kv.Key.ToLower(), kv.Value);
            }

            // Return all bodies within a given system 
            if (query.ContainsKey("system"))
            {
                return _context.StarSystem
                    .Where(ss => ss.NameLower.Equals(((string)query["system"]).ToLower()))
                    .SelectMany(ss => ss.Bodies);
            }

            // Return a single object automatically if name is set
            if (query.ContainsKey("name"))
            {
                return _context.Body.Where(b => b.Name.Equals(query["name"], StringComparison.OrdinalIgnoreCase));
            }

            //

            StarSystem rs = null;
            int maxDist = 100;

            throw new NotImplementedException();
        }

        /// <summary>
        /// GET api/v1/materials
        /// </summary>
        /// <returns></returns>
        [HttpGet("materials/{*matName}")]
        public IEnumerable<dynamic> GetMaterials(string matName)
        {
            // Get the material from the name
            var material = _context.Material.FirstOrDefault(m => string.Equals(m.Name, matName, StringComparison.OrdinalIgnoreCase));

            // Get and deserialize query from HTTP request
            Dictionary<string, StringValues> query = new Dictionary<string, StringValues>();
            foreach (KeyValuePair<string, StringValues> kv in Request.Query)
            {
                query.Add(kv.Key.ToLower(), kv.Value);
            }

            StarSystem rs = null;
            int maxDist = 100;

            // Get a reference system
            if (query.ContainsKey("refsystem"))
            {
                StarSystem s = _context.StarSystem.SingleOrDefault(ss => string.Equals(ss.Name, query["refsystem"], StringComparison.OrdinalIgnoreCase));
                if (s != null)
                {
                    rs = s;
                }
            }
            if (rs == null)
            {
                rs = _context.StarSystem.Single(ss => string.Equals(ss.Name, "sol", StringComparison.OrdinalIgnoreCase));
            }

            // Create bounds for bounding box
            double minx = rs.X - maxDist;
            double maxx = rs.X + maxDist;
            double miny = rs.Y - maxDist;
            double maxy = rs.Y + maxDist;
            double minz = rs.Z - maxDist;
            double maxz = rs.Z + maxDist;

            // Initial query using bounding box
            var systems = (from ss in _context.StarSystem
                           where minx <= ss.X && ss.X <= maxx
                              && miny <= ss.Y && ss.Y <= maxy
                              && minz <= ss.Z && ss.Z <= maxz
                           select ss);

            if (material == null) return null;

            if (material.Type.Equals("Raw")) // Raw material, find bodies
            {
                var results = systems.SelectMany(ss => ss.Bodies).Where(b => b.Materials.Any(m => m.MaterialId == material.Id));

                return results.Select(b => new
                {
                    systemName = b.StarSystem.Name,
                    bodyName = b.Name,
                    concentration = b.Materials.First(m => m.MaterialId == material.Id).Share,
                    systemDistance = b.StarSystem.DistanceTo(rs),
                    distanceToArrival = b.DistanceToArrival
                })
                .OrderBy(a => a.systemDistance).Take(100);
            }
            else if (material.Method == null)
            {
                return new List<dynamic> { new { whereToFind = material.MethodDescription } };
            }
            else if (material.Method.Contains("ELW"))
            {
                var results = systems.SelectMany(ss => ss.Bodies).Where(b => b.SubType.Contains("Earth"));

                return results.Select(b => new
                {
                    systemName = b.StarSystem.Name,
                    bodyName = b.Name,
                    systemAllegiance = b.StarSystem.Allegiance,
                    systemState = b.StarSystem.State,
                    systemSecurity = b.StarSystem.Security,
                    systemDistance = b.StarSystem.DistanceTo(rs),
                    whereToFind = material.MethodDescription
                })
                .OrderBy(a => a.systemDistance).Take(100);
            }
            else
            {
                var results = systems;
                bool hasState = true;
                //bool hasAnarchyOrSecurityLevel = false;
                foreach (string filter in material.Method.Split(','))
                {
                    var f = filter.Split(':');
                    switch (f[0])
                    {
                        case "State":
                            if (hasState)
                            {
                                results = results.Where(ss => ss.State.Equals(f[1].ToNormalCase(LookupOptions.States)));
                                hasState = false;
                            }
                            else
                            {
                                results = results.Union(systems.Where(ss => ss.State.Equals(f[1].ToNormalCase(LookupOptions.States))));
                            }
                            break;
                        case "Security":
                            results = results.Where(ss => ss.Security.Equals(f[1].ToNormalCase(LookupOptions.SecurityTypes)) || ss.Security.Equals("Anarchy"));
                            break;
                        case "Government":
                            results = results.Union(systems.Where(ss => ss.Government.Equals(f[1].ToNormalCase(LookupOptions.Governments))));
                            break;
                        case "Allegiance":
                            results = results.Where(ss => ss.Allegiance.Equals(f[1].ToNormalCase(LookupOptions.Allegiances)));
                            break;
                        default:
                            break;
                    }
                }

                return results.Select(ss => new
                {
                    systemName = ss.Name,
                    systemAllegiance = ss.Allegiance,
                    systemState = ss.State,
                    systemSecurity = ss.Security,
                    systemDistance = (float)ss.DistanceTo(rs),
                    whereToFind = material.MethodDescription
                })
                .OrderBy(a => a.systemDistance).Take(100);
            }
        }

        /// <summary>
        /// GET api/v1/systems?[parameters]
        /// </summary>
        /// <returns>A filtered list of StarSystems</returns>
        [HttpGet("systems")]
        public IEnumerable<StarSystem> GetSystems()
        {
            // Get and deserialize query from HTTP request
            Dictionary<string, StringValues> query = new Dictionary<string, StringValues>();
            foreach (KeyValuePair<string, StringValues> kv in Request.Query)
            {
                query.Add(kv.Key.ToLower(), kv.Value);
            }

            // Return single object automatically if name is set
            if (query.ContainsKey("name"))
            {
                return _context.StarSystem
                    .Where(ss => ss.NameLower.Equals(((string)query["name"]).ToLower()))
                    .Include(ss => ss.MinorFactionPresences);
            }

            StarSystem rs = null;
            IQueryable<StarSystem> results;
            int maxDist = 100;

            // Get a reference system
            if (query.ContainsKey("refsystem"))
            {
                StarSystem s = _context.StarSystem.SingleOrDefault(ss => ss.NameLower.Equals(((string)query["refsystem"]).ToLower()));
                if (s != null)
                {
                    rs = s;
                }
            }
            if (rs == null)
            {
                rs = _context.StarSystem.Single(ss => ss.NameLower.Equals("sol"));
            }

            // Get a max search distance
            if (query.ContainsKey("maxdistance") && int.TryParse(query["maxdistance"], out maxDist))
            {
                maxDist = maxDist > 200 ? 200 : maxDist;
            }

            // Create bounds for bounding box
            double minx = rs.X - maxDist;
            double maxx = rs.X + maxDist;
            double miny = rs.Y - maxDist;
            double maxy = rs.Y + maxDist;
            double minz = rs.Z - maxDist;
            double maxz = rs.Z + maxDist;

            // Initial query using bounding box
            results = (from ss in _context.StarSystem
                       where minx <= ss.X && ss.X <= maxx
                          && miny <= ss.Y && ss.Y <= maxy
                          && minz <= ss.Z && ss.Z <= maxz
                       select ss).Include(ss => ss.MinorFactionPresences);

            // Filter by whether system is populated
            if (query.ContainsKey("ispopulated") && bool.TryParse(query["ispopulated"], out bool ispop))
            {
                results = results.Where(ss => ss.IsPopulated == ispop);
            }

            // Filter by a minimum population
            if (query.ContainsKey("minpopulation") && int.TryParse(query["minpopulation"], out int pop))
            {
                results = results.Where(ss => ss.Population >= pop);
            }

            // Filter by allegiance
            if (query.ContainsKey("allegiance"))
            {
                results = results.Where(ss => ss.Allegiance.Equals(((string)query["allegiance"]).ToNormalCase(LookupOptions.Allegiances)));
            }

            // Filter by government
            if (query.ContainsKey("government"))
            {
                results = results.Where(ss => ss.Government.Equals(((string)query["government"]).ToNormalCase(LookupOptions.Governments)));
            }

            // Filter by system state
            if (query.ContainsKey("state"))
            {
                results = results.Where(ss => ss.State.Equals(((string)query["state"]).ToNormalCase(LookupOptions.States)));
            }

            // Filter by system security level
            if (query.ContainsKey("security"))
            {
                results = results.Where(ss => ss.Security.Equals(((string)query["security"]).ToNormalCase(LookupOptions.SecurityTypes)));
            }

            // Filter by primary economy
            if (query.ContainsKey("economy"))
            {
                results = results.Where(ss => ss.PrimaryEconomy.Equals(((string)query["economy"]).ToNormalCase(LookupOptions.Economies)));
            }

            // Filter by Power Play leader name
            if (query.ContainsKey("powername"))
            {
                results = results.Where(ss => ss.PowerPlayLeader.Equals(((string)query["powername"]).ToNormalCase(LookupOptions.PowerPlayLeaders)));
            }

            // Filter by Power Play system state
            if (query.ContainsKey("powerstate"))
            {
                results = results.Where(ss => ss.PowerPlayState.Equals(((string)query["powerstate"]).ToNormalCase(LookupOptions.PowerEffects)));
            }

            // Filter by mining reserves level
            if (query.ContainsKey("reserves"))
            {
                results = results.Where(ss => ss.Reserves.Equals(((string)query["reserves"]).ToNormalCase(LookupOptions.ReserveTypes)));
            }

            if (query.ContainsKey("page") && int.TryParse(query["page"], out int page)) { }
            else { page = 1; }

            return results.OrderBy(ss => ss.DistanceTo(rs)).Skip((page - 1) * 20).Take(20);
        }



        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }
    }
}
