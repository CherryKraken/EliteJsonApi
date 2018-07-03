using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EliteJsonApi.ViewModels
{
    public class MaterialTraderStationResult : IRawMaterialContainer, IEncodedMaterialContainer, IManufacturedMaterialContainer
    {
        public string Name { get; set; }
        public string Type => "Station";
        public string System { get; set; }
        public double SystemDistance { get; set; }
        public double DistanceToArrival { get; set; }
        public string Note => "Material availability may vary between station traders.";
    }
}
