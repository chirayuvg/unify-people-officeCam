using AppStudio.Framework.Core;
using System.ComponentModel.DataAnnotations;

namespace UnifyNow.Forest.Data.SQL
{
    public class TeamMember
    {
        [Key]
        [Required]
        public Guid Id { get; set; }
        public Guid? MemberId { get; set; }
        public Member? Member { get; set; }
        public Guid? TeamId { get; set; }
        public Team? Team { get; set; }

        [AutoFill(DefaultValueType = DefaultValueType.TenantId)]
        public Guid CustomerId { get; set; }
    }
}
