using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EliteJsonApi.Models
{
    public class Belt
    {
        [Key]
        public long Id { get; set; }
        [MaxLength(72)]
        public string Name { get; set; }
        [MaxLength(10)]
        public string BeltType { get; set; }
        public long? BeltMass { get; set; }
        public int? InnerRadius { get; set; }
        public int? OuterRadius { get; set; }

        public long StarSystemId { get; set; }
        [ForeignKey("StarSystemId")]
        public virtual StarSystem StarSystem { get; set; }
    }
}
