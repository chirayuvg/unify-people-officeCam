using System.ComponentModel.DataAnnotations;

namespace Unify.STR.Attendance.Data.SQL
{
    public class TasksHistory 
    {
        [Key]
        public Guid Id { get; set; }
        public Guid TasksId { get; set; }

        public Tasks Tasks { get; set; }


        public string Message { get; set; }

        public string FileUrl { get; set; }

        public string OriginalFileName { get; set; }
        public DateTime DateTime { get; set; }
    }
}
