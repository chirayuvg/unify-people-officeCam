
using System.ComponentModel.DataAnnotations;
using AppStudio.Framework.Core;

namespace Unify.STR.Attendance.Data.SQL
{
    public class Range 
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]

        public string Name { get; set; }

        public Guid? DivisionId { get; set; }
        public Division? Division { get; set; }

        [AutoFill(DefaultValueType = DefaultValueType.TenantId)]
        public Guid TenantId { get; set; }

    }
}
