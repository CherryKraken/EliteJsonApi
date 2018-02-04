using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EliteJsonApi.Models.Helpers.DataAnnotations
{
    public class LookupAttribute : ValidationAttribute
    {
        private readonly IEnumerable<string> _options;

        public LookupAttribute(LookupOption option)
        {
            switch (option)
            {
                case LookupOption.Allegiance:
                    _options = LookupOptions.Allegiances;
                    break;
                case LookupOption.Government:
                    _options = LookupOptions.Governments;
                    break;
                case LookupOption.State:
                    _options = LookupOptions.States;
                    break;
                case LookupOption.Economy:
                    _options = LookupOptions.Economies;
                    break;
                case LookupOption.ReserveType:
                    _options = LookupOptions.ReserveTypes;
                    break;
                case LookupOption.Security:
                    _options = LookupOptions.SecurityTypes;
                    break;
                case LookupOption.PowerEffect:
                    _options = LookupOptions.PowerEffects;
                    break;
                case LookupOption.PowerPlayLeader:
                    _options = LookupOptions.PowerPlayLeaders;
                    break;
                case LookupOption.BodyType:
                    _options = LookupOptions.BodyTypes;
                    break;
                case LookupOption.PresenceType:
                    _options = LookupOptions.PresenceTypes;
                    break;
                case LookupOption.MaterialGrade:
                    _options = LookupOptions.MaterialGrades;
                    break;
                case LookupOption.MaterialType:
                    _options = LookupOptions.MaterialTypes;
                    break;
            }
        }

        public override bool IsValid(object value)
        {
            var str = value as string;
            return _options.Contains(str.ToLower());
        }
    }

    public enum LookupOption
    {
        Allegiance,
        Government,
        State,
        Economy,
        Security,
        PowerPlayLeader,
        PowerEffect,
        PresenceType,
        ReserveType,
        BodyType,
        MaterialType,
        MaterialGrade,

    }
}

