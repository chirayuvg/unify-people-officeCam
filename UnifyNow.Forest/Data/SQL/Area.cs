using AppStudio.Framework.Core;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace UnifyNow.Forest.Data.SQL
{
    public class Area
    {
       [Key]
        [Required]
        public Guid Id { get; set; }

        public string Name { get; set; }
        public string Polygon { get; set; }

        public string MapPolygon { get; set; }

        public Guid? CameraId { get; set; }
        public Camera? Camera { get; set; }
    }
}
