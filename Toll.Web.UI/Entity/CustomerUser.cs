using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toll.Web.UI.Entity
{
    [Index(nameof(Email), nameof(CustomerId), IsUnique = true)]
    [Index(nameof(Name), nameof(CustomerId), IsUnique = true)]
    public class CustomerUser
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        //[Required]
        //[DataType(DataType.Password)]
        //[MaxLength(50)]
        //public string Password { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? PhoneNo { get; set; } = string.Empty;

        public List<string> UserRoles { get; set; } = [];

        public bool IsActive { get; set; } = true;

        public Guid CustomerId { get; set; }
        public Customer? Customer { get; set; }


    }
}
