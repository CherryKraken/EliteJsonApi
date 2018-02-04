using System.ComponentModel.DataAnnotations.Schema;

namespace EliteJsonApi.Models
{
    public class RawMaterialShare
    {
        public long BodyId { get; set; }
        [ForeignKey("BodyId")]
        public virtual Body Body { get; set; }

        public int MaterialId { get; set; }
        [ForeignKey("MaterialId")]
        public virtual Material Material { get; set; }

        public float Share { get; set; }
    }
}