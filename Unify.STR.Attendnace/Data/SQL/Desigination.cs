using AppStudio.Framework.Core;
using System.ComponentModel.DataAnnotations;

namespace Unify.STR.Attendance.Data.SQL
{
    public class Desigination 
    {

        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }


        [AutoFill(DefaultValueType = DefaultValueType.TenantId)]
        public Guid TenantId { get; set; }
    }
}
