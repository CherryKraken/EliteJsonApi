using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EliteJsonApi.ViewModels
{
    /// <summary>
    /// Class for taking query requests from a posted form
    /// </summary>
    public class MaterialFormViewModel
    {
        [EnumDataType(typeof(MaterialType))]
        public MaterialType Type { get; set; }

        public string[] MaterialNames { get; set; }

        public string ReferenceSystem { get; set; }
    }

    public enum MaterialType
    {
        Raw,
        Encoded,
        Manufactured
    }
}
