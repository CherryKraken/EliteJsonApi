using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EliteJsonApi.Models
{
    public class Ring
    {
        [Key]
        [MaxLength(100)]
        public string Name { get; set; }
        [MaxLength(20)]
        public string Type { get; set; }
        public int? InnerRadius { get; set; }
        public int? OuterRadius { get; set; }
        public long? Mass { get; set; }

        public long BodyId { get; set; }
        [ForeignKey("BodyId")]
        public virtual Body Body { get; set; }
    }
}