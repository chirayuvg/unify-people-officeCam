using System.ComponentModel.DataAnnotations;

namespace UnifyNow.Forest.Data.SQL
{
    public class ForestImages
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

      
        public string ImageUrl { get; set; }


        public DateTime CreatedDate { get; set; }
    }
}
