using AppBuilder.Framework.Core;
using AppBuilder.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toll.Web.UI.Entity
{
    [Index(nameof(Name), nameof(CustomerId), IsUnique = true)]
    public class Camera
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        [Display(GroupName = "[General]")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Display(GroupName = "[General]")]
        public string? Description { get; set; } = string.Empty;
        [Display(GroupName = "[General]")]
        public Guid CameraTypeId { get; set; }

        public CameraType? CameraType { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(GroupName = "[General]")]
        public string ConnectionString { get; set; } = string.Empty;

        [Display(GroupName = "[Location]")]
        public Guid BranchId { get; set; }
        public Branch? Branch { get; set; }
        [Display(GroupName = "[Location]")]
        public Guid SectorId { get; set; }
        public Sector? Sector { get; set; }
        [Display(GroupName = "[Location]")]
        public Guid? AreaId { get; set; }
        public Area? Area { get; set; }

        [Display(GroupName = "[Asset]")]
        public Guid? AssetId { get; set; }
        // public Asset? Asset { get; set; }

        //Camera AOI
        //public string AOI { get; set; } = string.Empty;
        [Display(GroupName = "[Calibration]")]
        public int CalibartionPixels { get; set; }
        [Display(GroupName = "[Calibration]")]
        public decimal CalibrationDistance { get; set; }
        [Display(GroupName = "[Calibration]")]
        public DistanceUOM CalibrationDistanceUOM { get; set; }
        [Display(GroupName = "[General]")]
        public bool IsActive { get; set; } = false;
        public string? Tag { get; set; } = string.Empty;


        [AutoFill(DefaultValueType = DefaultValueType.TenantId)]
        public Guid CustomerId { get; set; }
    }
    public enum DistanceUOM
    {
        MM,
        CM,
        Inches
    }
}
