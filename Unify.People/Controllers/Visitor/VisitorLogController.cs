using AppStudio.Model;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Unify.Data;
using Unify.Data.SQL;
using Unify.People.Services;

namespace Unify.People.Controllers.Visitors
{
    [Authorize("CookiePolicy")]
    public class VisitorLogController : Controller
    {
        private readonly AppointmentServices _appointmentServices;
        private readonly VisitorLogServices _visitorLogServices;
        private readonly ILogger<VisitorLogController> _logger;

        public VisitorLogController(AppointmentServices appointmentServices, VisitorLogServices visitorLogServices, ILogger<VisitorLogController> logger)
        {
            _appointmentServices = appointmentServices;
            _visitorLogServices = visitorLogServices;
            _logger = logger;
        }

        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> Index()
        {
            var model = new CheckInModel();
            model.AppointmentId = new Guid(Request.Query["AppointmentId"]);

            var peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();

            ViewBag.StartVisitor = peopleDb.VisitorLogs.Count(m => m.AppointmentId.Equals(model.AppointmentId));
            ViewBag.NoOfVisitors = peopleDb.Appointments.Where(m => m.Id.Equals(model.AppointmentId)).SingleOrDefault().NoOfVisitors;

            return PartialView(model);
        }

        [HttpGet]
        public IActionResult GetEmployees(DataSourceLoadOptions loadOptions)
        {
            var peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();

            var data = peopleDb.Employees.Select(e => new {
                id = e.Id.ToString(),
                name = e.Name
            }).ToList();

            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetVisitors(DataSourceLoadOptions loadOptions, Guid AppointmentId, Guid VisitorId)
        {
            var peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();
            var getLatestAppointment = peopleDb.Appointments
                .Where(a=>a.Id== AppointmentId)
                .OrderByDescending(m => m.AppointmentDate)
                .ThenByDescending(m => m.AppointmentTime)
                
                .FirstOrDefault();
            var getLatestVisitor = peopleDb.VisitorLogs.Where(m => m.AppointmentId == AppointmentId).OrderByDescending(m => m.AppointmentDate).ThenByDescending(m => m.AppointmentTime).FirstOrDefault();
            var data = Enumerable.Empty<VisitorLog>(); // Or new List<VisitorLog>();

            if (getLatestVisitor != null)
            {
                data = await _visitorLogServices.GetVisitorLogByAppointmentIdVisitorIdAsync(AppointmentId, getLatestVisitor.VisitorId);
            }
            else
            {
                // You can leave it as is or log something if needed
                data = await _visitorLogServices.GetVisitorLogByAppointmentIdAsync(AppointmentId);
            }

            return Json(DataSourceLoader.Load(data, loadOptions));
        }

        [HttpPost]
        public async Task<IActionResult> Capture(CheckInModel checkInModel)
        {
            await _visitorLogServices.ValidateImage(checkInModel);

            if (checkInModel.VisitorId == Guid.Empty)
            {
                return await CreateVisitor(checkInModel);
            }
            else
            {
                return await ConfirmVisitor(checkInModel);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateVisitor(CheckInModel checkInModel)
        {
            checkInModel.ImageBase64String = await ImageBase64String(checkInModel.Image);
            return PartialView("CreateVisitor", checkInModel);
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmVisitor(CheckInModel checkInModel)
        {
            checkInModel.ImageBase64String = await ImageBase64String(checkInModel.Image);
            return PartialView("ConfirmVisitor", checkInModel);
        }

        [HttpPost]
        public async Task<IActionResult> InsertVisitor(CheckInModel checkInModel)
        {
            string message = "";

            if (checkInModel.VisitorName == null || checkInModel.VisitorName == "")
            {
                message += "- Please enter Visitor Name.\\n";
            }
            if (checkInModel.MobileNo == null || checkInModel.MobileNo == 0)
            {
                message += "- Please enter Mobile Number.\\n";
            }
            if (checkInModel.MobileNo != null && checkInModel.MobileNo.ToString().Length < 10)
            {
                message += "- Please enter valid Mobile Number.\\n";
            }
            if (checkInModel.Company == null || checkInModel.Company == "")
            {
                message += "- Please enter Company Name.\\n";
            }
            if (checkInModel.BadgeNo == null || checkInModel.BadgeNo == "")
            {
                message += "- Please enter Badge Number.\\n";
            }
            if (!string.IsNullOrEmpty(message))
            {
                UiAction.AddJsCode("alert('" + message + "');");

                var returnResponse = UiAction.JavascriptResult.GetJavascriptResult(this);
                return new JsonResult(
                     returnResponse,
                     new JsonSerializerOptions
                     {
                         Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                         PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                     });
            }

            checkInModel.Image = await GetImageFormFile(checkInModel.ImageBase64String);
            string returnValue = await _visitorLogServices.InsertVisitor(checkInModel);

            if (string.IsNullOrEmpty(returnValue))
            {
                UiAction.AddJsCode("$('#checkInGrid').dxDataGrid('instance').refresh(); $('#divCreateVisitor').hide(); increaseCounter();");
            }
            else
            {
                UiAction.AddJsCode("$('#divUpdateVisitor').hide(); alert('{" + returnValue + "}');");
            }

            var response = UiAction.JavascriptResult.GetJavascriptResult(this);
            return new JsonResult(
                 response,
                 new JsonSerializerOptions
                 {
                     Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                     PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                 });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateVisitor(CheckInModel checkInModel)
        {
            string message = "";

            if (checkInModel.VisitorName == null || checkInModel.VisitorName == "")
            {
                message += "- Please enter Visitor Name.\\n";
            }
            if (checkInModel.MobileNo == null || checkInModel.MobileNo == 0)
            {
                message += "- Please enter Mobile Number.\\n";
            }
            if (checkInModel.MobileNo != null && checkInModel.MobileNo.ToString().Length < 10)
            {
                message += "- Please enter valid Mobile Number.\\n";
            }
            if (checkInModel.Company == null || checkInModel.Company == "")
            {
                message += "- Please enter Company Name.\\n";
            }
            if (checkInModel.BadgeNo == null || checkInModel.BadgeNo == "")
            {
                message += "- Please enter Badge Number.\\n";
            }
            if (!string.IsNullOrEmpty(message))
            {
                UiAction.AddJsCode("alert('" + message + "');");

                var returnResponse = UiAction.JavascriptResult.GetJavascriptResult(this);
                return new JsonResult(
                     returnResponse,
                     new JsonSerializerOptions
                     {
                         Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                         PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                     });
            }

            checkInModel.Image = await GetImageFormFile(checkInModel.ImageBase64String);
            string returnValue = await _visitorLogServices.UpdateVisitor(checkInModel);

            if (string.IsNullOrEmpty(returnValue))
            {
                UiAction.AddJsCode("$('#checkInGrid').dxDataGrid('instance').refresh(); $('#divUpdateVisitor').hide(); increaseCounter();");
            }
            else if (returnValue == "DUPLICATE_VISITOR")
            {
                message = "This visitor already has checked in. Please try check in the next Visitor.";
                UiAction.AddJsCode("$('#divUpdateVisitor').hide(); alert('" + message + "');");
            }
            else if (returnValue == "DUPLICATE_BADGE_NO")
            {
                message = "Badge Number \"" + checkInModel.BadgeNo + "\" has already alloted. Please allot another Badge Number.";
                UiAction.AddJsCode("$('#divUpdateVisitor').hide(); alert('" + message + "');");
            }
            else
            {
                UiAction.AddJsCode("$('#divUpdateVisitor').hide(); alert('{" + returnValue + "}');");
            }

            var response = UiAction.JavascriptResult.GetJavascriptResult(this);
            return new JsonResult(
                 response,
                 new JsonSerializerOptions
                 {
                     Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                     PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                 });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateVisitorLog(Guid key, string values)
        {
            Guid AppointmentId = await _visitorLogServices.UpdateVisitorLog(key, values);
            UiAction.AddJsCode("$('#checkInGrid').dxDataGrid('instance').refresh();");

            var response = UiAction.JavascriptResult.GetJavascriptResult(this);
            return new JsonResult(
                 response,
                 new JsonSerializerOptions
                 {
                     Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                     PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                 });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteVisitorLog(Guid key)
        {
            Guid AppointmentId = await _visitorLogServices.DeleteVisitorLog(key);
            UiAction.AddJsCode("$('#checkInGrid').dxDataGrid('instance').refresh(); decreaseCounter();");

            var response = UiAction.JavascriptResult.GetJavascriptResult(this);
            return new JsonResult(
                 response,
                 new JsonSerializerOptions
                 {
                     Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                     PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                 });
        }

        [HttpGet]
        public async Task<IActionResult> PrintGatePass(Guid AID)
        {
            try
            {
                QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

                var peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();
                var appointment = await peopleDb.Appointments
                    .Include(m => m.Employee)
                    .FirstOrDefaultAsync(m => m.Id == AID);

                if (appointment == null)
                    return NotFound("Appointment not found.");

                var visitorLogs = await _visitorLogServices.GetVisitorLogByAppointmentIdAsync(AID);

                // Start the meeting
                await _appointmentServices.StartInMeeting(AID);

                // Prepare receipt text
                string receiptDetails =
                    "\n" +
                    "                   Company Name\n" +
                    "\n" +
                    "              Address of the Company\n" +
                    "==================================================\n" +
                    "                   GATE PASS\n" +
                    "Date and Time: " + DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss") + "\n" +
                    "\n" +
                    "From Company: " + appointment.Company + "\n" +
                    "\n" +
                    "Badge No            Visitor Name\n" +
                    "---------------     ------------------------------\n";

                foreach (var visitorLog in visitorLogs)
                {
                    receiptDetails += visitorLog.BadgeNo.PadRight(20) + visitorLog.VisitorName + "\n";
                }

                receiptDetails +=
                    "==================================================\n" +
                    "Appointment With:   " + appointment.Employee?.Name + "\n" +
                    "\n" +
                    "Signature:\n" +
                    "\n" +
                    "\n" +
                    "\n" +
                    "==================================================";

                // Define file path
                string wwwrootBase = await FindWwwRootFolder();
                string folderName = "VisitorPass";
                string fileName = AID.ToString() + ".pdf";
                string folderPath = Path.Combine(wwwrootBase, folderName);

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string outputPath = Path.Combine(folderPath, fileName);

                // Generate the PDF
                var document = new QuestPdfDocument(receiptDetails);
                document.GeneratePdf(outputPath);

                var pdfBytes = await System.IO.File.ReadAllBytesAsync(outputPath);
                return File(pdfBytes, "application/pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Gate Pass PDF");

                return Ok("Error");
            }
        }

        private async Task<string> ImageBase64String(IFormFile Image)
        {
            var memoryStream = new MemoryStream();
            await Image.CopyToAsync(memoryStream);
            byte[] fileBytes = memoryStream.ToArray();

            return Convert.ToBase64String(fileBytes);
        }

        private async Task<IFormFile> GetImageFormFile(string ImageBase64String)
        {
            byte[] fileBytes = Convert.FromBase64String(ImageBase64String);
            var stream = new MemoryStream(fileBytes);

            IFormFile image = new FormFile(stream, 0, fileBytes.Length, "Image", "capture.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentDisposition = "form-data; name=\"Image\"; filename=\"capture.jpg\"",
                ContentType = "image/jpeg"
            };

            return image;
        }

        private async Task<string> FindWwwRootFolder()
        {
            string currentDir = AppContext.BaseDirectory;

            while (!string.IsNullOrEmpty(currentDir))
            {
                string wwwrootPath = Path.Combine(currentDir, "wwwroot");
                if (Directory.Exists(wwwrootPath)) return wwwrootPath;
                currentDir = Directory.GetParent(currentDir)?.FullName;
            }

            throw new DirectoryNotFoundException("wwwroot folder not found.");
        }
    }

    public class CheckInModel
    {
        public Guid CheckInLogId { get; set; }

        public Guid AppointmentId { get; set; }

        public float Latitude { get; set; }

        public float Longitude { get; set; }

        public IFormFile Image { get; set; }

        public string ImageBase64String { get; set; }

        public Guid VisitorId { get; set; }

        [Required]
        [MaxLength(100, ErrorMessage = "Length cannot exceed 100 characters.")]
        public string VisitorName { get; set; } = string.Empty;

        [Required]
        public Int64? MobileNo { get; set; }

        [Required]
        [MaxLength(50, ErrorMessage = "Length cannot exceed 50 characters.")]
        public string Company { get; set; }

        [MaxLength(250, ErrorMessage = "Length cannot exceed 250 characters.")]
        public string? Address { get; set; }

        [MaxLength(50, ErrorMessage = "Length cannot exceed 50 characters.")]
        [EmailAddress]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid email address")]
        public string? Email { get; set; }

        [Required]
        public string? BadgeNo { get; set; }
    }
}
