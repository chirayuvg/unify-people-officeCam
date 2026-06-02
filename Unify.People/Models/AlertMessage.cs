using AppStudio.Model;
using System.ComponentModel.DataAnnotations;

namespace Unify.People.Models
{
    public class AlertMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Message { get; set; }
        public Severity Severity { get; set; }
        public string Status { get; set; }
        public string Action { get; set; }

        //public Guid UserId { get;set; }
        public DateTime CreatedTime { get; set; }

        //[AutoFill(DefaultValueType = DefaultValueType.TenantId)]
        public Guid TenantId { get; set; }
    }

    public enum Severity
    {
        Critical,
        Major,
        Minor,
        Warning,
        Information
    }
}
