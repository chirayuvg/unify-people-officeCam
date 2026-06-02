using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toll.Web.UI.Entity
{
    [Index(nameof(Name), IsUnique = true)]
    public class Customer
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? Address { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? BillingAddress { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string? Phone { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string? ContactPerson { get; set; } = string.Empty;

        public List<CustomerUser>? CustomerUsers { get; set; } = [];

        public bool IsActive { get; set; } = true;

        public Guid TenantId { get; set; }
        public Tenant? Tenant { get; set; }
    }
}
