using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EliteJsonApi.Models.Helpers.DataAnnotations
{

    public class LookupAttribute : ValidationAttribute
    {
        private readonly string[] _options;

        public LookupAttribute(string options)
        {
            _options = options.Split(',');
        }

        public override bool IsValid(object value)
        {
            var str = value as string;
            return _options.Contains(str.ToLower());
        }
    }

}
