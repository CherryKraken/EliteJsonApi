using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EliteJsonApi.Models.Helpers
{
    public static class LookupOptions
    {
        public static readonly IEnumerable<string> Allegiances = new string[]
        {
            "Alliance", "Empire", "Federation", "Independent", "None", "Pilots Federation", "Pirate"
        };

        public static readonly IEnumerable<string> Governments = new string[]
        {
            "Anarchy", "Communism", "Confederacy", "Corporate", "Cooperative", "Democracy", "Dictatorship", "Feudal", "Imperial", "Patronage", "Prison Colony", "Theocracy", "None", "Engineer"
        };

        public static readonly IEnumerable<string> Economies = new string[]
        {
            "Agriculture", "Extraction", "High Tech", "Industrial", "Military", "Refinery", "Service", "Terraforming", "Tourism", "Colony", "None"
        };

        public static readonly IEnumerable<string> States = new string[]
        {
            "Boom", "Bust", "Famine", "Civil Unrest", "Civil War", "Election", "Expansion", "Lockdown", "Outbreak", "War", "None", "Retreat", "Investment"
        };

        public static readonly IEnumerable<string> PresenceTypes = new string[]
        {
            "Presence", "Controlling", "None"
        };

        public static readonly IEnumerable<string> PowerEffects = new string[]
        {
            "Control", "Exploited", "Expansion", "None"
        };

        public static readonly IEnumerable<string> SecurityTypes = new string[]
        {
            "Low", "Medium", "High", "Anarchy", "Lawless"
        };

        public static readonly IEnumerable<string> PowerPlayLeaders = new string[]
        {
            "Aisling Duval", "Archon Delaine", "Arissa Lavigny-Duval", "Denton Patreus", "Edmund Mahon", "Felicia Winters", "Li Yong-Rui", "Pranav Antal", "Yuri Grom", "Zachary Hudson", "Zemina Torval", "None"
        };

        public static readonly IEnumerable<string> ReserveTypes = new string[]
        {
            "Pristine", "Major", "Common", "Depleted", "None"
        };

        public static readonly IEnumerable<string> MaterialTypes = new string[]
        {
            "Data", "Manufactured", "Raw"
        };

        public static readonly IEnumerable<string> MaterialGrades = new string[]
        {
            "1", "2", "3", "4", "5"
        };

        /// <summary>
        /// Do not use on imports to ensure there are actually just two types
        /// </summary>
        public static readonly IEnumerable<string> BodyTypes = new string[]
        {
            "Star", "Planet" // In future, add options to filter sub types as well
        };
        
        /// <summary>
        /// Tries to convert the string to match the case of the options in the given IEnumerable.
        /// If the target does not exist, returns the target string.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static string ToNormalCase(this string target, IEnumerable<string> options)
        {
            if (target == null)
            {
                if (options.Contains("None"))
                {
                    return "None";
                }
                else if (options.Contains("Low"))
                {
                    return "Low";
                }
            }

            foreach (string s in options)
            {
                if (s.Equals(target, StringComparison.OrdinalIgnoreCase))
                {
                    return s;
                }
            }

            return target;
        }

        /// <summary>
        /// Converts a list of string options to a CSV list
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static string AsCsv(this IEnumerable<string> options)
        {
            return string.Join(',', options).ToLower();
        }
    }
}
