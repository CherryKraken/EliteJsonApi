using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EliteJsonApi.Models
{
    public class Planet : Body
    {
        [MaxLength(40)]
        public string AtmosphereType { get; set; }
        public double? EarthMasses { get; set; }
        public double? Gravity { get; set; }
        public bool? IsLandable { get; set; }
        public float? Radius { get; set; }
        public double? SurfacePressure { get; set; }
        [MaxLength(40)]
        public string TerraformingState { get; set; }
        [MaxLength(40)]
        public string VolcanismType { get; set; }
    }
}
