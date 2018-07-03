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
        public double Concentration { get; set; }
    }
}
