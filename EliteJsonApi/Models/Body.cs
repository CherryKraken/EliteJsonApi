using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EliteJsonApi.Models
{
    public abstract class Body
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

        public double? ArgOfPeriapsis { get; set; }
        public double? AxialTilt { get; set; }
        public double DistanceToArrival { get; set; }
        public bool IsTidallyLocked { get; set; }
        public double? OrbitalEccentricity { get; set; }
        public float? OrbitalInclination { get; set; }
        public double? OrbitalPeriod { get; set; }
        public double? RotationalPeriod { get; set; }
        public double? SemiMajorAxis { get; set; }
        public int SurfaceTemperature { get; set; }

        public long StarSystemId { get; set; }
        [ForeignKey("StarSystemId")]
        [JsonIgnore]
        public virtual StarSystem StarSystem { get; set; }

        [JsonIgnore]
        public virtual ICollection<RawMaterialShare> Materials { get; set; }
        public virtual ICollection<Ring> Rings { get; set; }

        public virtual ICollection<AtmosphereComposite> AtmosphereComposition { get; set; }
    }
}
