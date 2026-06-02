using AppStudio.Framework.Core;
using System.ComponentModel.DataAnnotations;

namespace Unify.Data.SQL
{
    public class Organization
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        // Departments
        public List<Department> Departments { get; set; } = [];

        // Designations
        public List<Designation> Designations { get; set; } = [];

        // Employee Types
        public List<EmployeeType> EmployeeTypes { get; set; } = [];

        // Shifts
        public List<Shift> Shifts { get; set; } = [];

        // PersonType
        [AutoFill(DefaultValueType = DefaultValueType.TenantId)]
        public Guid TenantId { get; set; }
    }
}
