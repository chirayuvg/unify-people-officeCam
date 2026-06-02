using AppStudio.Framework.Core;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace UnifyNow.Forest.Data.SQL
{
    [Index(nameof(Name), nameof(CustomerId), IsUnique = true)]
    public class Animal
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }


        [AutoFill(DefaultValueType = DefaultValueType.TenantId)]
        public Guid CustomerId { get; set; }
    }
}
