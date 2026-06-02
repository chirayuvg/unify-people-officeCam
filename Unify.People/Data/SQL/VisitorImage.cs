using AppStudio.Framework.Core;
using Pgvector;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Unify.Data.SQL
{
    public class VisitorImage
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [MaxLength(250)]
        [DataType(DataType.ImageUrl)]
        public string Image { get; set; }

        public Guid? VisitorId { get; set; }
        public Visitor? Visitor { get; set; }

        [AutoFill(DefaultValueType = DefaultValueType.TenantId)]
        public Guid TenantId { get; set; }

        [Column(TypeName = "vector(512)")]
        public Vector Embedding { get; set; }
    }
}
