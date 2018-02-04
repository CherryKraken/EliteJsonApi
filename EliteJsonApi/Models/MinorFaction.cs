using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EliteJsonApi.Models.Helpers.DataAnnotations;
using Newtonsoft.Json;

namespace EliteJsonApi.Models
{
    [JsonObject(ItemReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
    public class MinorFaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; } // Use EDDB's Id
        
        [MaxLength(60)]
        public string Name { get; set; }

        public DateTime UpdatedAt { get; set; }

        public bool IsPlayerFaction { get; set; }

        [MaxLength(20)]
        [Lookup(LookupOption.Government)]
        public string Government { get; set; }

        [MaxLength(30)]
        [Lookup(LookupOption.Allegiance)]
        public string Allegiance { get; set; }

        [MaxLength(20)]
        [Lookup(LookupOption.State)]
        public string State { get; set; }

        [ForeignKey("HomeSystem")]
        public long HomeSystemId { get; set; }
        public virtual StarSystem HomeSystem { get; set; }

        public virtual ICollection<MinorFactionPresence> MinorFactionPresences { get; set; }
        [JsonIgnore]
        [InverseProperty("ControllingMinorFaction")]
        public virtual ICollection<StarSystem> MinorFactionControlSystems { get; set; }
    }
}
