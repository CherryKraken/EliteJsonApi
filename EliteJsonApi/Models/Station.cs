using EliteJsonApi.Models.Helpers.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EliteJsonApi.Models
{
    public class Station
    {
        [Key]
        public long Id { get; set; }
        public string Name { get; set; }

        public long ControllingMinorFactionId { get; set; }
        [ForeignKey("ControllingMinorFactionId")]
        public MinorFaction ControllingMinorFaction { get; set; }

        [Lookup(LookupOption.Economy)]
        public string Economy { get; set; }

        #region Facilities

        public bool HasMissionBoard { get; set; }
        public bool HasOutfitting { get; set; }
        public bool HasShipyard { get; set; }
        public bool HasRefuel { get; set; }
        public bool HasRepair { get; set; }
        public bool HasRestock { get; set; }
        public bool HasInterstellarFactors { get; set; }
        public bool HasCommoditiesMarket { get; set; }
        public bool HasTechBroker { get; set; }
        public bool HasEncodedMaterialTrader { get; set; }
        public bool HasRawMaterialTrader { get; set; }
        public bool HasManufacturedMaterialTrader { get; set; }
        [NotMapped]
        public bool HasMaterialTrader => HasEncodedMaterialTrader || HasRawMaterialTrader || HasManufacturedMaterialTrader;

        #endregion

    }
}
