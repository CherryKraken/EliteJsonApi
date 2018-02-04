using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EliteJsonApi.Models.Helpers.DataAnnotations
{
    public static class LookupOptions
    {
        public const string Allegiance = "alliance,empire,federation,independent,none,pilots federation,pirate";
        public const string Government = "anarchy,communism,confederacy,corporate,cooperative,democracy,dictatorship,feudal,imperial,patronage,prison colony,theocracy,none,engineer";
        public const string Economy = "agriculture,extraction,high tech,industrial,military,refinery,service,terraforming,tourism,colony,none";
        public const string State = "boom,bust,famine,civil unrest,civil war,election,expansion,lockdown,outbreak,war,none,retreat,investment";
        public const string PresenceType = "presence,controlling,none";
        public const string PowerEffect = "control,exploited,expansion,none";
        public const string Security = "low,medium,high,anarchy,lawless";
        public const string PowerPlayLeader = "aisling duval,archon delaine,arissa lavigny-duval,denton patreus,edmund mahon,felicia winters,li yong-rui,yuri grom,zachary hudson,zemina torval,none";
        public const string ReserveType = "pristine,major,common,depleted,none";
        public const string MaterialType = "raw,data,manufactured";
        public const string Grades = "1,2,3,4,5";
    }
}
