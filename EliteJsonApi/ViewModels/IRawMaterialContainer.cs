using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EliteJsonApi.ViewModels
{
    /// <summary>
    /// Marker interface for game location types that may contain raw materials (i.e. Bodies and Stations with Raw Material Trader)
    /// </summary>
    public interface IRawMaterialContainer
    {
        /// <summary>
        /// The name of the container (i.e. the body name or station name)
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The container's type (i.e. body or station)
        /// </summary>
        string Type { get; }

        /// <summary>
        /// The name of the container's star system
        /// </summary>
        string System { get; set; }

        /// <summary>
        /// The calculated distance of the container's star system
        /// </summary>
        double SystemDistance { get; set; }

        /// <summary>
        /// The distance from the system navigation beacon to the container
        /// </summary>
        double DistanceToArrival { get; set; }
    }
}
