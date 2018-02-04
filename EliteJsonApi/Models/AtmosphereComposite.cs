using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EliteJsonApi.Models
{
    public class AtmosphereComposite
    {
        [Key]
        public string Component { get; set; }
        public float? Share { get; set; }

        [Key]
        public long BodyId { get; set; }
        [ForeignKey("BodyId")]
        public virtual Body Body { get; set; }
    }
}