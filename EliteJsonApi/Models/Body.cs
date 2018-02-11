using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EliteJsonApi.Models
{
    public class Body
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }
        public long? EddbId { get; set; }
        public long? EdsmId { get; set; }

        [MaxLength(64)]
        public string Name { get; set; }
        [MaxLength(20)]
        public string Type { get; set; }
        [MaxLength(50)]
        public string SubType { get; set; }

        #region Stars

        public uint? Age { get; set; }
        public float? AbsoluteMagnitude { get; set; }
        public bool? IsMainStar { get; set; }
        public bool? IsScoopable { get; set; }
        public float? SolarMasses { get; set; }
        public double? SolarRadius { get; set; }

        /// <summary>
        /// A, Ae/Be, B, C, D, F, G, K, L, M, O, T, Y, etc.
        /// </summary>
        [MaxLength(5)]
        public string SpectralClass { get; set; }
        /// <summary>
        /// I - VII
        /// </summary>
        [MaxLength(5)]
        public string LuminosityClass { get; set; }
        /// <summary>
        /// a, a0, ab, z, etc.
        /// </summary>
        [MaxLength(5)]
        public string LuminositySubClass { get; set; }

        #endregion

        #region Planets and moons

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

        #endregion

        public double? ArgOfPeriapsis { get; set; }
        public double? AxialTilt { get; set; }
        public double? DistanceToArrival { get; set; }
        public bool? IsTidallyLocked { get; set; }
        public double? OrbitalEccentricity { get; set; }
        public float? OrbitalInclination { get; set; }
        public double? OrbitalPeriod { get; set; }
        public double? RotationalPeriod { get; set; }
        public double? SemiMajorAxis { get; set; }
        public int? SurfaceTemperature { get; set; }

        #region Nagigation properties

        public long StarSystemId { get; set; }
        [ForeignKey("StarSystemId")]
        [JsonIgnore]
        public virtual StarSystem StarSystem { get; set; }
        
        public virtual ICollection<RawMaterialShare> Materials { get; set; }
        public virtual ICollection<Ring> Rings { get; set; }
        public virtual ICollection<AtmosphereComposite> AtmosphereComposition { get; set; }

        #endregion
    }
}
