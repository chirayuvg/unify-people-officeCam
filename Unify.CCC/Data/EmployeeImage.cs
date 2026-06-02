
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using AppStudio.Framework.Core;

namespace Unify.CCC.Data
{
    public class EmployeeImage
    {

        [Key]
        [Required]
        public Guid Id { get; set; }

        [MaxLength(250)]
        [DataType(DataType.Upload)]
        public string Image { get; set; }
        public string? Imageurl { get; set; }

        //public List<double>? Embeddings { get; set; }

        //public List<Embedding>? Embedding { get; set; }
        //public Embedding Embedding { get; set; }

        public Guid? EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        [AutoFill(DefaultValueType = DefaultValueType.TenantId)]
        public Guid CustomerId { get; set; }

        //[NotMapped]
        //public Embeddings Embeddings { get; set; } = new();

        //public string EmbeddingsJson
        //{
        //    get => JsonSerializer.Serialize(Embeddings);
        //    set
        //    {
        //        if (value == null) return;
        //        Embeddings = JsonSerializer.Deserialize<Embeddings>(value)!;
        //    }
        //}

        [NotMapped]
        internal Embeddings? Embeddings { get; set; } = new();

        //internal List<double>? Embeddings { get; set; } = new();

        internal string? EmbeddingsJson
        {
            get => JsonSerializer.Serialize(Embeddings);
            set
            {
                if (value == null) return;
                Embeddings = JsonSerializer.Deserialize<Embeddings>(value)!;
            }
        }
    }

    public class Embeddings
    {
        public List<double> Embedding { get; set; }
    }

    public class Embedding
    {
        public List<double> Embeddings { get; set; }
    }



}