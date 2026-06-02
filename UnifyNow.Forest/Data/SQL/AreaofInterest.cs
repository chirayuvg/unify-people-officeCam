using System.ComponentModel.DataAnnotations;

namespace UnifyNow.Forest.Data.SQL
{
    public class AreaofInterest
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        public string Name { get; set; }
        public string Polygon { get; set; }

        public string MapPolygon { get; set; }


        //public Guid? PresetId { get; set; }
        //public Preset? Preset { get; set; }
    }
}
