using System.ComponentModel.DataAnnotations;

namespace UnifyNow.Forest.Data.SQL
{
    public class Ticket
    {
        [Key]
        [Required]
        public Guid Id { get; set; }
        public Int64 TicketId { get; set; }
        [Required]
        [MaxLength(50)]
        public string Title { get; set; }

        [Required]
        public Guid? CameraId { get; set; }
        public Camera? Camera { get; set; }

        [Required]
        public Guid? AnimalId { get; set; }
        public Animal? Animal { get; set; }

        [Required]
        public Guid? AreaId { get; set; }
        public Area? Area { get; set; }

        [Required]
        public Guid? MemberId { get; set; }
        public Member? Member { get; set; }

        public string Type { get; set; }    

        public string Remark { get; set; }

        public DateTime CreatedDate { get; set; }

        public string Status { get; set; }

        public string fileURL { get; set; }
    }
}
