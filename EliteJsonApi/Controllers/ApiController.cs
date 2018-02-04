using EliteJsonApi.Data;
using EliteJsonApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

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
            throw new NotImplementedException();
        }

        /// <summary>
        /// GET api/v1/materials
        /// </summary>
        /// <returns></returns>
        [HttpGet("materials")]
        public IEnumerable<dynamic> GetMaterials()
        {
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

            if (query.ContainsKey("name"))
            {
                Material mat = _context.Material.FirstOrDefault(m => string.Equals(m.Name, query["name"], StringComparison.OrdinalIgnoreCase));
                if (mat == null) return null;

                if (mat.Type.Contains("aw")) // Raw material, find bodies
                {
                    var results = _context.Body.Include(b => b.StarSystem).Include(b => b.Materials).Where(b => b.Materials.Select(m => m.MaterialId).Contains(mat.Id));
                    return results.Select(b => new
                    {
                        system_name = b.StarSystem.Name,
                        body_name = b.Name,
                        concentration = b.Materials.First(m => m.MaterialId == mat.Id).Share,
                        system_distance = b.StarSystem.DistanceTo(rs),
                        distance_to_arrival = b.DistanceToArrival
                    })
                    .OrderBy(a => a.system_distance).Take(20);
                }
            }

            return null;
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
                    .Where(ss => string.Equals(ss.Name, query["name"], StringComparison.OrdinalIgnoreCase))
                    .Include(ss => ss.MinorFactionPresences);
            }

            StarSystem rs = null;
            IQueryable<StarSystem> results;
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

            // Filter by allegiance
            if (query.ContainsKey("allegiance"))
            {
                results = results.Where(ss => string.Equals(ss.Allegiance, query["allegiance"], StringComparison.OrdinalIgnoreCase));
            }

            // Filter by government
            if (query.ContainsKey("government"))
            {
                results = results.Where(ss => string.Equals(ss.Government, query["government"], StringComparison.OrdinalIgnoreCase));
            }

            // Filter by system state
            if (query.ContainsKey("state"))
            {
                results = results.Where(ss => string.Equals(ss.State, query["state"], StringComparison.OrdinalIgnoreCase));
            }

            // Filter by system security level
            if (query.ContainsKey("security"))
            {
                results = results.Where(ss => string.Equals(ss.Security, query["security"], StringComparison.OrdinalIgnoreCase));
            }

            // Filter by primary economy
            if (query.ContainsKey("economy"))
            {
                results = results.Where(ss => string.Equals(ss.PrimaryEconomy, query["economy"], StringComparison.OrdinalIgnoreCase));
            }

            // Filter by Power Play leader name
            if (query.ContainsKey("powername"))
            {
                results = results.Where(ss => string.Equals(ss.PowerPlayLeader, query["powername"], StringComparison.OrdinalIgnoreCase));
            }

            // Filter by Power Play system state
            if (query.ContainsKey("powerstate"))
            {
                results = results.Where(ss => string.Equals(ss.PowerPlayState, query["powerstate"], StringComparison.OrdinalIgnoreCase));
            }

            // Filter by mining reserves level
            if (query.ContainsKey("reserves"))
            {
                results = results.Where(ss => string.Equals(ss.Reserves, query["reserves"], StringComparison.OrdinalIgnoreCase));
            }

            if (query.ContainsKey("page") && int.TryParse(query["page"], out int page)) { }
            else { page = 1; }

            return results.OrderBy(ss => ss.DistanceTo(rs));//.Skip((page - 1) * 20).Take(20);
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
