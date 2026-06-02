using System.ComponentModel.DataAnnotations;

namespace Unify.STR.Attendance.Data.SQL
{
    public class Tasks 
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public Guid UserId { get; set; }

        public DateTime DateTime { get; set; }

    }
}
