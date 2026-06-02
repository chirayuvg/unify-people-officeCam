using System.ComponentModel.DataAnnotations;
using AppStudio.Framework.Core;


namespace Unify.STR.Attendance.Data.SQL
{
    public class Circle 
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]

        public string Name { get; set; }

        [AutoFill(DefaultValueType = DefaultValueType.TenantId)]
        public Guid TenantId { get; set; }

    }
}
