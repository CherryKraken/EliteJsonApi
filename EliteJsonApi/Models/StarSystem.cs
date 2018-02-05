using EliteJsonApi.Models.Helpers.DataAnnotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EliteJsonApi.Models
{
    public class StarSystem
    {
        //[Key] // Removed be attribute because EF doesn't like its references in MinorFaction
        public long Id { get; set; }
        public long? EddbId { get; set; }
        public long? EdsmId { get; set; }

        [Required]
        [MaxLength(64)]
        public string Name { get; set; }

        /// <summary>
        /// Property for indexing the Name property to lowercase in the database
        /// </summary>
        [MaxLength(64)]
        [JsonIgnore]
        public string NameLower { get => Name.ToLower(); set => NameLower = value; }

        #region Coordinates
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        #endregion

        public bool NeedsPermit { get; set; }
        public DateTime UpdatedAt { get; set; }
        [MaxLength(30)]
        public string SimbadRef { get; set; }
        public bool IsPopulated { get; set; }
        public long Population { get; set; }

        [MaxLength(30)]
        [Lookup(LookupOption.Allegiance)]
        public string Allegiance { get; set; }

        [MaxLength(10)]
        [Lookup(LookupOption.Security)]
        public string Security { get; set; }

        [MaxLength(10)]
        [Lookup(LookupOption.ReserveType)]
        public string Reserves { get; set; }

        [MaxLength(30)]
        [Lookup(LookupOption.Economy)]
        public string PrimaryEconomy { get; set; }

        [MaxLength(20)]
        [Lookup(LookupOption.Government)]
        public string Government { get; set; }

        [MaxLength(20)]
        [Lookup(LookupOption.State)]
        public string State { get; set; }

        [MaxLength(30)]
        [Lookup(LookupOption.PowerPlayLeader)]
        public string PowerPlayLeader { get; set; }

        [MaxLength(30)]
        [Lookup(LookupOption.PowerEffect)]
        public string PowerPlayState { get; set; }
        

        [ForeignKey("ControllingMinorFaction")]
        public long? ControllingMinorFactionId { get; set; }
        [JsonIgnore]
        public virtual MinorFaction ControllingMinorFaction { get; set; }
        public virtual ICollection<MinorFactionPresence> MinorFactionPresences { get; set; }
        [JsonIgnore]
        public virtual ICollection<Body> Bodies { get; set; }
        [JsonIgnore]
        public virtual ICollection<Belt> Belts { get; set; }

        [JsonIgnore]
        [InverseProperty("HomeSystem")]
        public virtual ICollection<MinorFaction> MinorFactionHQs { get; set; }

        //[NotMapped]
        //public float Distance { get; private set; }


        /// <summary>
        /// Calculates the distance to a given system in light-years
        /// </summary>
        /// <param name="destination">Another system</param>
        /// <returns>The distance in light-years from this system to the given system</returns>
        public double DistanceTo(StarSystem destination)
        {
            return /*Distance = */(float)Math.Sqrt(Math.Pow(destination.X - this.X, 2)
                           + Math.Pow(destination.Y - this.Y, 2)
                           + Math.Pow(destination.Z - this.Z, 2));
        }

    }
}
