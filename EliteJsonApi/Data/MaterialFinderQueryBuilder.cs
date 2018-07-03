using EliteJsonApi.Models;
using EliteJsonApi.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EliteJsonApi.Data
{
    public class MaterialFinderQueryBuilder
    {
        private readonly EliteDbContext _context;

        public MaterialFinderQueryBuilder(EliteDbContext context)
        {
            _context = context;
        }

        public RawMaterialQueryResultList FindRawMaterials(IQueryCollection queryString, string[] matNames)
        {
            // Get the material from the name
            var materials = new List<Material>();
            foreach (string matName in matNames)
                materials.Add(_context.Material
                    .FirstOrDefault(m => string.Equals(m.Name, matName, StringComparison.OrdinalIgnoreCase)));

            // Get and deserialize query from HTTP request
            Dictionary<string, StringValues> query = new Dictionary<string, StringValues>();
            foreach (KeyValuePair<string, StringValues> kv in queryString)
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

            if (materials.Count == 0) return null;

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
    }
}
