using EliteJsonApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EliteJsonApi.ViewModels
{
    public class RawMaterialBodyResult : IRawMaterialContainer
    {
        public string Name { get; set; }
        public string Type => "Body";
        public string System { get; set; }
        public double SystemDistance { get; set; }
        public double DistanceToArrival { get; set; }
        public Dictionary<string, double> Concentrations { get; set; } = new Dictionary<string, double>();
    }

    public static class RawMaterialConcentrationExtensions
    {
        public static Dictionary<string, double> MapConcentrationsFor(this IEnumerable<Material> materials, Body body)
        {
            if (body.Materials == null)
                throw new Exception("ICollection<RawMaterialShare> Body.Materials was not loaded.");

            var dict = new Dictionary<string, double>();
            foreach (var material in materials)
            {
                dict.Add(material.Name, body.Materials.First(m => m.MaterialId == material.Id).Share);
            }

            return dict;
        }
    }
}
