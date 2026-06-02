using AppStudio.Enterprise.Data;
using AppStudio.Enterprise.Services;
using AppStudio.Framework.Core;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using Unify.STR.Attendance.Data.SQL;

namespace Unify.STR.Attendance.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        [HttpPost("upload-employees")]
        public async Task<IActionResult> UploadEmployees(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is empty.");

            var employees = new List<Employee>();

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.First();
            var rows = worksheet.RangeUsed().RowsUsed().Skip(1);
            foreach (var row in rows)
            {
                Employee employee = new();
                employee.Id = Guid.NewGuid(); // Generate new ID for each employee
                employee.Name = row.Cell(1).GetString();
                employee.Code = row.Cell(2).GetString();
                if (row.Cell(3).GetString().ToLower() == "male")
                {
                    employee.Gender = Gender.Male;
                }
                else if (row.Cell(3).GetString().ToLower() == "female")
                {
                    employee.Gender = Gender.Female;
                }
                employee.DoB = DateOnly.Parse(row.Cell(4).GetString());
                employee.DoJ = DateOnly.Parse(row.Cell(5).GetString());
                employee.EmployeeTypeId = Guid.Parse(row.Cell(6).GetString());
                employee.AgencyName = row.Cell(7).GetString();
                employee.CircleId = Guid.Parse(row.Cell(8).GetString());
                employee.DivisionId = Guid.Parse(row.Cell(9).GetString());
                employee.RangeId = Guid.Parse(row.Cell(10).GetString());
                employee.SectionId = Guid.Parse(row.Cell(11).GetString());
                employee.BeatId = Guid.Parse(row.Cell(12).GetString());
                employee.ShiftId = Guid.Parse(row.Cell(13).GetString());
                employee.MobileNo = row.Cell(14).GetString();
                employee.Email = row.Cell(15).GetString();

                var result = await Entity.InsertAsync(employee);
                string password = string.Empty;

                if (employee.MobileNo != null)
                {
                    string mobileStr = employee.MobileNo.ToString();
                    password = mobileStr.Length >= 4 ? mobileStr.Substring(mobileStr.Length - 4) : mobileStr;
                }
                TenantUser tenantUser = new()
                {
                    Id = employee.Id,
                    Name = employee.Name,
                    Email = (employee.MobileNo != null ? employee.MobileNo.ToString() : "") + "@str.com",
                    Password = password,
                    IsActive = true,
                    TenantId = employee.TenantId,
                };
                var service = CoreServices.GetService<TenantUserService>();
                var res = await service.CreateIdentityUserAsync(tenantUser);
            }
            return Ok(new { Message = "Employees uploaded successfully." });
        }
        
    }
}
