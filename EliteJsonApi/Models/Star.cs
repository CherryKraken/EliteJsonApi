using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EliteJsonApi.Models
{
    public class Star : Body
    {
        public uint? Age { get; set; }
        public float? AbsoluteMagnitude { get; set; }
        public bool IsMainStar { get; set; }
        public bool IsScoopable { get; set; }
        public float? SolarMasses { get; set; }
        public double? SolarRadius { get; set; }
        [MaxLength(2)]
        public string SpectralClass { get; set; } // KGBFOAM, etc.
        [MaxLength(3)]
        public string LuminosityClass { get; set; } // I - VII
        [MaxLength(4)]
        public string LuminositySubClass { get; set; } // a, a0, ab, z, etc.
    }
}
