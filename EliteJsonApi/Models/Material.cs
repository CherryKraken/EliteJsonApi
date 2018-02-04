using EliteJsonApi.Models.Helpers.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EliteJsonApi.Models
{
    public class Material
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(40)]
        public string Name { get; set; }
        [MaxLength(15)]
        [Lookup(LookupOptions.MaterialType)]
        public string Type { get; set; }
        [MaxLength(1)]
        [Lookup(LookupOptions.Grades)]
        public string Grade { get; set; }

        public ICollection<RawMaterialShare> RawMaterialShares { get; set; }
    }
}
