using AppStudio.Framework.Core;
using System.ComponentModel.DataAnnotations;

namespace Unify.Data.SQL
{
    public class Regularization
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTime ApplyDateTime { get; set; }

        public Guid EmployeeId { get; set; }

        public Guid? AttendanceId { get; set; }

        public DateOnly RegularizationDate { get; set; }

        public TimeOnly? StartTime { get; set; }

        public TimeOnly? EndTime { get; set; }

        public bool? Approved { get; set; }

        public Guid? ApprovedById { get; set; }

        [AutoFill(DefaultValueType = DefaultValueType.TenantId)]
        public Guid TenantId { get; set; }
    }
}
