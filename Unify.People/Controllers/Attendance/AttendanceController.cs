using Microsoft.AspNetCore.Mvc;
using Unify.People.Services;

namespace Unify.People.Controllers.Attendance
{
    public class AttendanceController : Controller
    {
        private readonly AttendanceServices _attendanceServices;
        private readonly ILogger<AttendanceController> _logger;

        public AttendanceController(AttendanceServices attendanceServices, ILogger<AttendanceController> logger)
        {
            _attendanceServices = attendanceServices;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return PartialView();
        }



        [HttpGet]
        public IActionResult Capture(string Tenent)
        {
            var model = new AttModel();
            return PartialView(model);
        }

        [HttpPost]
        public async Task<IActionResult> Capture(AttModel model)
        {
            await _attendanceServices.LoginAsync(model);
            return Confirm(model);
        }

        [HttpGet]
        public IActionResult Confirm(AttModel model)
        {
            return PartialView("Confirm",model);
        }
    }

    public class AttModel
    {
        public float Latitude { get; set; }

        public float Longitude { get; set; }

        public IFormFile Image { get; set; }

        public string Action { get; set; }

        public string EmployeeCode { get; set; } = string.Empty;

        public string EmployeeName { get; set; }=string.Empty;

        public string Designation { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string EmployeeType { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;    

        public string ErorrMessage { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = string.Empty;

        public Guid? TenantId { get; set; }

        public float[]? Embedding { get; set; }
    }
}
