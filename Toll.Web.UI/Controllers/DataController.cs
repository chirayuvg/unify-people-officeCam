using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Toll.Web.UI.Controllers
{
    public class DataController : Controller
    {
        private readonly TollDbUIContext _context;
        public DataController(TollDbUIContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return PartialView();
        }

        public ActionResult ExporttoExcel()
        {
           
                var billingData = _context.VehicleBillings.Include(m => m.Booth).Include(m => m.Shift).Include(m => m.VehicleType).Include(m => m.VehicleCategory).Where(m => m.CreatedOn.Date == DateTime.Now.Date).ToList();
                //var billingData = _context.VehicleBillings.Include(m => m.Booth).Include(m => m.Shift).Include(m => m.VehicleType).Include(m => m.VehicleCategory).ToList();

            var workbook = new XLWorkbook();
                var worksheet = workbook.AddWorksheet("Billing Details");
                worksheet.Cell(1, 1).Value = "TicketNumber";
                worksheet.Cell(1, 2).Value = "VehicleNumber";
                worksheet.Cell(1, 3).Value = "Shift";
                worksheet.Cell(1, 4).Value = "Booth";
                worksheet.Cell(1, 5).Value = "VehicleType";
                worksheet.Cell(1, 6).Value = "VehicleCategory";
                worksheet.Cell(1, 7).Value = "TollAmount";
                worksheet.Cell(1, 8).Value = "TotalAmount";
                for (int i = 0; i < billingData.Count; i++)
                {
                    worksheet.Cell(i + 2, 1).Value = billingData[i].TicketNumber;
                    worksheet.Cell(i + 2, 2).Value = billingData[i].VehicleNumber;
                    worksheet.Cell(i + 2, 3).Value = billingData[i].Shift.Name;
                    worksheet.Cell(i + 2, 4).Value = billingData[i].Booth.Name;
                    worksheet.Cell(i + 2, 5).Value = billingData[i].VehicleType.Name;
                    worksheet.Cell(i + 2, 6).Value = billingData[i].VehicleCategory.Category;
                    worksheet.Cell(i + 2, 7).Value = billingData[i].TollAmount;
                    worksheet.Cell(i + 2, 8).Value = billingData[i].TotalAmount;

                }
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "VehicleDetails.xlsx");
                }
            }

        }
    }


