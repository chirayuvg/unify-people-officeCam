using AppStudio.Enterprise.Data;
using AppStudio.Framework.Core;
using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Data.ResponseModel;
using DevExtreme.AspNet.Mvc;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using System.ComponentModel.DataAnnotations;
using Unify.Data;
using Unify.Data.SQL;
using Unify.People.Services;

namespace Unify.People.Controllers.Attendance
{
    public class AttendanceLogsController : PageController
    {
        private readonly AttendanceServices _attendanceServices;
        private readonly EmployeeService _employeeService;

        private CardView<DetectionLogsFilterData> cardView;
        private GridView<Unify.Data.SQL.AttendanceLog> gridView;

        private PopupView popupAddNew;
        private PopupView popupMapTo;

        private CardView<Employee> employeeAddNew;
        private CardView<MapTo> employeeMapTo;

        [ViewState]
        public string attendanceLogId { get; set; }

        public AttendanceLogsController(AttendanceServices attendanceServices, EmployeeService employeeService)
        {
            _attendanceServices = attendanceServices;
            _employeeService = employeeService;
        }

        public override void Init()
        {
            this.Page.SetTitle("Attendance Logs");

            cardView = this.AddCardView<DetectionLogsFilterData>().SetControlStyle(CardViewStyle.Filter);
            cardView.ControlStyle = CardViewStyle.Filter;
            cardView.LabelPosition = LabelPosition.Left;
            cardView.OnSubmit += CardView_OnSubmit;

            if (cardView.Data.Period == DetetionLogsPeriod.CustomDateRange)
            {
                cardView.GetFieldControl(m => m.FromDate).IsReadOnly = false;
                cardView.GetFieldControl(m => m.ToDate).IsReadOnly = false;
            }
            else
            {
                cardView.GetFieldControl(m => m.FromDate).IsReadOnly = true;
                cardView.GetFieldControl(m => m.ToDate).IsReadOnly = true;
            }

            gridView = this.AddGridView<Unify.Data.SQL.AttendanceLog>()
                .ConfigueUI(
                    grid => grid
                        .ColumnAutoWidth(false)
                        .FilterRow(f => f.Visible(false))
                        .HeaderFilter(headerFilter => headerFilter.Visible(false))
                        .RowAlternationEnabled(true)
                        .Scrolling(scrolling => scrolling.ColumnRenderingMode(GridColumnRenderingMode.Virtual))
                        .SearchPanel(s => s.Visible(true).HighlightCaseSensitive(false))
                        .ShowBorders(true)
                        .ShowColumnHeaders(true)
                        .ShowColumnLines(false)
                        .ShowRowLines(false)
                        .WordWrapEnabled(true).Width("100%")
                        .Paging(paging => paging.Enabled(true).PageSize(10))
                        .Pager(pager =>
                        {
                            pager
                                .Visible(true)
                                .DisplayMode(GridPagerDisplayMode.Full)
                                .ShowPageSizeSelector(false)
                                .AllowedPageSizes(new JS("[5]"))
                                .ShowInfo(true)
                                .ShowNavigationButtons(true);
                        })
                );
            gridView.Css.SetHeight("calc(100vh - 290px)");
            gridView.Fields(field =>
            {
                field.ClearFields();

                field.AddField(f => f.Image);
                field.AddField(f => f.Code);
                field.AddField(f => f.Name);
                field.AddField(f => f.Action);
                field.AddField(f => f.DateTime);
            });
            gridView.SetDefaultSort("DateTime", true);
            gridView.OnLoadData += GridView_OnLoadData;

            employeeAddNew = new CardView<Employee>();
            employeeAddNew.OnSubmit += EmployeeAddNew_OnSubmit;
            var btnAddNew = gridView.AddButton("Add New");
            btnAddNew.OnClick += BtnAddNew_OnClick;
            popupAddNew = this.AddPopupView();
            popupAddNew.SetId("popupAddNew");
            popupAddNew.Title = "Add New";
            popupAddNew.AddControl(employeeAddNew);

            employeeMapTo = new CardView<MapTo>();
            employeeMapTo.Css.SetWidth("500px");
            employeeMapTo.OnSubmit += EmployeeMapTo_OnSubmit;
            var btnMapTo = gridView.AddButton("Map To");
            btnMapTo.OnClick += BtnMapTo_OnClick;
            popupMapTo = this.AddPopupView();
            popupMapTo.SetId("popupMapTo");
            popupMapTo.Title = "Map To";
            popupMapTo.AddControl(employeeMapTo);
        }

        private async Task CardView_OnSubmit(DetectionLogsFilterData model, IFormFileCollection files)
        {
            model.GetDates();

            if (cardView.Data.Period == DetetionLogsPeriod.CustomDateRange)
            {
                cardView.GetFieldControl(m => m.FromDate).IsReadOnly = false;
                cardView.GetFieldControl(m => m.ToDate).IsReadOnly = false;
            }
            else
            {
                cardView.GetFieldControl(m => m.FromDate).IsReadOnly = true;
                cardView.GetFieldControl(m => m.ToDate).IsReadOnly = true;
            }
        }

        private async Task<LoadResult> GridView_OnLoadData(DataSourceLoadOptions loadOptions, string? parentId)
        {
            var TenantId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetTenantId();
            var UserId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetUserId();

            DateTime fromDate =
                (cardView.Data.FromDate == DateOnly.MinValue)
                ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"))
                : DateTime.Parse(cardView.Data.FromDate.ToString("yyyy-MM-dd"));
            DateTime toDate =
                (cardView.Data.ToDate == DateOnly.MinValue)
                ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"))
                : DateTime.Parse(cardView.Data.ToDate.ToString("yyyy-MM-dd"));
            string personType = cardView.Data.GetPersonType();
            string cameraName = cardView.Data.GetCameraName();

            TenantUser tenantUser = await Entity.FindByIdAsync<TenantUser>(UserId);
            var peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();
            IQueryable<AttendanceLog> query;

            if (tenantUser.Roles.Any(r => (r == "Super Admin") || (r == "Admin")))
            {
                query = peopleDb.AttendanceLogs
                    .Where(m =>
                           m.TenantId == TenantId
                        && m.DateTime.Date >= fromDate.Date
                        && m.DateTime.Date <= toDate.Date
                        && m.Image != ""
                    );
            }
            else
            {
                var employee = await _employeeService.GetEmployeeByEmailAsync(TenantId, tenantUser.Email);

                query = peopleDb.AttendanceLogs
                    .Where(m =>
                           m.TenantId == TenantId
                        && m.DateTime.Date >= fromDate.Date
                        && m.DateTime.Date <= toDate.Date
                        && m.Image != ""
                        && ((m.EmployeeId.ToString() == "00000000-0000-0000-0000-000000000000") || (m.EmployeeId == employee.Id))
                    );
            }

            if (!string.IsNullOrEmpty(cameraName)) query = query.Where(ai => ai.CameraName == cameraName);

            var results = query
                .Select(ai => new AttendanceIndex
                {
                    Id = ai.Id,
                    Image = ai.Image,
                    EmployeeId = ai.EmployeeId,
                    Code = string.IsNullOrEmpty(ai.Code) ? "" : ai.Code,
                    Name = string.IsNullOrEmpty(ai.Name) ? "" : ai.Name,
                    Action = ai.Action,
                    CameraName = ai.CameraName,
                    Time = ai.TimeStamp.ToString("HH:mm:ss"),
                    Date = ai.DateTime.ToString("yyyy-MM-dd"),
                    DateTime = ai.DateTime,
                    EmployeeType = ai.EmployeeType,
                    PersonType = (ai.EmployeeId == Guid.Empty) ? "Unidentified" : "Employee"
                })
                .ToList();

            if (personType != "All") results = results
                    .Where(m => m.PersonType.Equals(personType)).ToList()
                    .ToList();

            foreach (var result in results)
            {
                result.Date = DateTime.Parse(result.Date).ToString("dd-MMM-yy");
                result.Time = TimeOnly.Parse(result.Time).ToString("HH:mm:ss");
            }

            return DataSourceLoader.Load(results, loadOptions);
        }

        private void BtnAddNew_OnClick(ButtonItem obj)
        {
            attendanceLogId = obj.RecordId;
            employeeAddNew.Data.Name = "TEST";
            employeeAddNew.Data.Code = "";
            employeeAddNew.Data.Gender = Gender.Male;
            employeeAddNew.Data.DoB = DateOnly.FromDateTime(DateTime.MinValue);
            employeeAddNew.Data.EmployeeTypeId = new Guid();
            employeeAddNew.Data.DepartmentId = new Guid();
            employeeAddNew.Data.DesignationId = new Guid();
            employeeAddNew.Data.ShiftId = new Guid();
            employeeAddNew.Data.MobileNo = null;
            employeeAddNew.Data.Email = "";
            popupAddNew.Show();
        }

        private void BtnMapTo_OnClick(ButtonItem obj)
        {
            attendanceLogId = obj.RecordId;
            employeeMapTo.Data.EmployeeId = new Guid();
            popupMapTo.Show();
        }

        private async Task EmployeeAddNew_OnSubmit(Employee model, IFormFileCollection files)
        {
            //  ----------------------------------------------------------------------------------------------------
            //  Create new Employee
            //  ----------------------------------------------------------------------------------------------------
            var result = await Entity.InsertAsync(model);
            //  ----------------------------------------------------------------------------------------------------
            //  Assign Employee details to AttendanceLog
            //  ----------------------------------------------------------------------------------------------------
            Vector Embedding = await _attendanceServices.AssignEmployeeToAttendanceLog(attendanceLogId, model);
            //  ----------------------------------------------------------------------------------------------------
            //  Create new image record for Employee, taking the Embedding from AttendanceLog
            //  ----------------------------------------------------------------------------------------------------
            if (Embedding != null)
            {
                EmployeeImage employeeImage = new EmployeeImage();
                employeeImage.Image = "Photo.PNG";
                employeeImage.EmployeeId = model.Id;
                employeeImage.TenantId = model.TenantId;
                employeeImage.Embedding = Embedding;
                var empImgRep = CoreServices.GetRepository<EmployeeImage>();
                await empImgRep.InsertAsync(employeeImage);
            }

            UiAction.ClosePopup();
        }

        private async Task EmployeeMapTo_OnSubmit(MapTo model, IFormFileCollection files)
        {
            //  --------------------------------------------------
            //  Fetch Employee Details
            //  --------------------------------------------------
            var employee = await Entity.Query<Employee>()
                .Include(m => m.EmployeeType)
                .Include(m => m.Department)
                .Include(m => m.Designation)
                .Include(m => m.Shift)
                .FirstOrDefaultAsync(m => m.Id == model.EmployeeId);

            //  ----------------------------------------------------------------------------------------------------
            //  Assign Employee details to AttendanceLog
            //  ----------------------------------------------------------------------------------------------------
            Vector Embedding = await _attendanceServices.AssignEmployeeToAttendanceLog(attendanceLogId, employee);

            //  ----------------------------------------------------------------------------------------------------
            //  Create new image record for Employee, taking the Embedding from AttendanceLog
            //  ----------------------------------------------------------------------------------------------------
            if (Embedding != null)
            {
                EmployeeImage employeeImage = new EmployeeImage();
                employeeImage.Image = "Photo.PNG";
                employeeImage.EmployeeId = employee.Id;
                employeeImage.TenantId = employee.TenantId;
                employeeImage.Embedding = Embedding;

                var empImgRep = CoreServices.GetRepository<EmployeeImage>();
                await empImgRep.InsertAsync(employeeImage);

                //Fetch AttendanceLog Image stream
                var fileStorage = CoreServices.FileStorageService;

                //var hostingEnv = ((AppStudio.Framework.Core.LocalFileStorageServices)fileStorage).HostingEnvironment;

                var path = Path.Combine("AttendanceLog", "Image");
                var attendanceLogImageStream = await fileStorage.GetFileAsync("documents", path, attendanceLogId);

                //  Save the same image in Employee Image folder
                path = Path.Combine(nameof(employeeImage), nameof(employeeImage.Image));
                await fileStorage.SaveFileAsync(
                    "documents"
                    , path
                    , employeeImage.Id.ToString()
                    , MimeHelper.GetMimeType("jpg")
                    , attendanceLogImageStream
                );
            }

            UiAction.ClosePopup();
        }
    }

    public class DetectionLogsFilterData
    {
        public EmployeePersonType PersonType { get; set; } = EmployeePersonType.All;

        public Guid CameraId { get; set; } = Guid.Empty;

        public DetetionLogsPeriod Period { get; set; } = DetetionLogsPeriod.Today;

        public DateOnly FromDate { get; set; }

        public DateOnly ToDate { get; set; }

        public DetectionLogsFilterData()
        {
            FromDate = DateOnly.FromDateTime(DateTime.Today);
            ToDate = DateOnly.FromDateTime(DateTime.Today);
        }

        public string GetPersonType()
        {
            return PersonType switch
            {
                EmployeePersonType.All => "All",
                EmployeePersonType.Employee => "Employee",
                EmployeePersonType.Unidentified => "Unidentified",
                _ => throw new NotImplementedException()
            };
        }

        public string GetCameraName()
        {
            //var _peopleDb = AppStudio.Framework.Core.CoreServices.GetService<Unify.CCC.Data.CCCDb>();
            //var camera = (CameraId == Guid.Empty) ? "" : _peopleDb.Cameras.Where(m => m.Id == CameraId).FirstOrDefault().Name;
            //return camera;
            return null;
        }

        public void GetDates()
        {
            DateOnly fromDate = DateOnly.FromDateTime(DateTime.Today);
            DateOnly toDate = DateOnly.FromDateTime(DateTime.Today);

            switch (Period)
            {
                case DetetionLogsPeriod.Today:
                    fromDate = DateOnly.FromDateTime(DateTime.Today);
                    toDate = DateOnly.FromDateTime(DateTime.Today);
                    break;
                case DetetionLogsPeriod.ThisWeek:
                    fromDate = fromDate.AddDays(-(int)fromDate.DayOfWeek + (int)System.DayOfWeek.Monday);
                    toDate = fromDate.AddDays(6);
                    break;
                case DetetionLogsPeriod.ThisMonth:
                    fromDate = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);
                    toDate = fromDate.AddMonths(1).AddDays(-1);
                    break;
                case DetetionLogsPeriod.LastMonth:
                    fromDate = new DateOnly(DateTime.Now.AddMonths(-1).Year, DateTime.Now.AddMonths(-1).Month, 1);
                    toDate = fromDate.AddMonths(1).AddDays(-1);
                    break;
                case DetetionLogsPeriod.CustomDateRange:
                    fromDate = DateOnly.Parse(FromDate.ToString());
                    toDate = DateOnly.Parse(ToDate.ToString());
                    if (ToDate < FromDate)
                    {
                        UiAction.AddJsCode("alert('\"To Date\" must be greater than or equals to \"From Date\"');");
                        fromDate = DateOnly.FromDateTime(DateTime.Today);
                        toDate = DateOnly.FromDateTime(DateTime.Today);


                        break;
                    }
                    break;
                default:
                    break;
            }

            FromDate = DateOnly.Parse(fromDate.ToString("yyyy-MM-dd"));
            ToDate = DateOnly.Parse(toDate.ToString("yyyy-MM-dd"));
        }
    }

    public class AttendanceIndex
    {
        [Key]
        public Guid? Id { get; set; }

        [MaxLength(250, ErrorMessage = "Length cannot exceed 250 characters.")]
        [DataType(DataType.ImageUrl)]
        public string Image { get; set; }

        public Guid? EmployeeId { get; set; }

        public string? Code { get; set; }

        public string? Name { get; set; }

        public string? Action { get; set; }

        public string? CameraName { get; set; }

        public string? Time { get; set; }

        public string? Date { get; set; }

        public DateTime DateTime { get; set; }

        public string EmployeeType { get; set; }

        public string? PersonType { get; set; }
    }

    public enum DetetionLogsPeriod
    {
        Today,
        ThisWeek,
        ThisMonth,
        LastMonth,
        CustomDateRange
    }

    public enum EmployeePersonType
    {
        All,
        Employee,
        Unidentified,
    }

    public enum VisitorPersonType
    {
        All,
        Unidentified,
        Visitor,
    }

    public class MapTo
    {
        public Guid EmployeeId { get; set; } = Guid.Empty;
        public Employee? Employee { get; set; }
    }
}
