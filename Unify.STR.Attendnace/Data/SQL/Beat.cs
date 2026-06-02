using System.ComponentModel.DataAnnotations;
using AppStudio.Framework.Core;

namespace Unify.STR.Attendance.Data.SQL
{
    public class Beat 
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]

        public string Name { get; set; }

        [Required]
        public string Polygon { get; set; }

        public Guid? SectionId { get; set; }
        public Section? Section { get; set; }

        [AutoFill(DefaultValueType = DefaultValueType.TenantId)]
        public Guid TenantId { get; set; }

    }
}
