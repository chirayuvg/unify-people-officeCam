using AppStudio.Framework.Core;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Unify.Data.SQL
{

    [Index(nameof(Name), nameof(TenantId), IsUnique = true)]
    public class Visitor
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Invalid mobile number.")]
        public Int64? MobileNo { get; set; }

        [Required(ErrorMessage = "Company is required")]
        [MaxLength(50)]
        public string Company { get; set; }

        [MaxLength(250)]
        public string? Address { get; set; }

        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email format.")]
        [MaxLength(50)]
        public string? Email { get; set; }

        [AutoFill(DefaultValueType = DefaultValueType.TenantId)]
        public Guid TenantId { get; set; }

        public List<VisitorImage> VisitorImages { get; set; } = [];
    }

}


