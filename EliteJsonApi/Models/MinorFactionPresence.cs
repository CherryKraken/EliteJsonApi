using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace EliteJsonApi.Models
{
    [JsonObject(ItemReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
    public class MinorFactionPresence
    {
        [JsonIgnore]
        public long StarSystemId { get; set; }
        [ForeignKey("StarSystemId")]
        public virtual StarSystem StarSystem { get; set; }
        
        public long MinorFactionId { get; set; }
        [ForeignKey("MinorFactionId")]
        [JsonIgnore]
        public virtual MinorFaction MinorFaction { get; set; }

        public double Influence { get; set; }
    }
}