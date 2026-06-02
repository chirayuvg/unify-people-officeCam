using AppStudio.Framework.Core;
using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Data.ResponseModel;
using DevExtreme.AspNet.Mvc;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System.ComponentModel.DataAnnotations;
using Unify.STR.Attendance.Data.SQL;
using Unify.STR.Attendance.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Range = Unify.STR.Attendance.Data.SQL.Range;


namespace Unify.STR.Attendance.Controllers.Attendance
{
    public class AttendanceLogsController : PageController
    {
        private CardView<DetectionLogsFilterData> cardView;
        private GridView<AttendanceLog> gridView;

        private PopupView popupAddNew;
        private PopupView popupMapTo;

        [ViewState]
        public string attendanceLogId { get; set; }

        private readonly EmployeeService _employeeService;

        public override void Init()
        {
            cardView = this.AddCardView<DetectionLogsFilterData>().SetControlStyle(CardViewStyle.Filter);
            cardView.ControlStyle = CardViewStyle.Filter;
            cardView.LabelPosition = LabelPosition.Left;
            cardView.OnSubmit += CardView_OnSubmit;

            gridView = this.AddGridView<AttendanceLog>();
            gridView
                .ConfigueUI(
                    grid => grid
                        .ColumnAutoWidth(false)
                        .FilterRow(f => f.Visible(false))
                        .HeaderFilter(headerFilter => headerFilter.Visible(false))
                        .RowAlternationEnabled(true)
                        .Scrolling(scrolling => scrolling.ColumnRenderingMode(GridColumnRenderingMode.Virtual))
                        .SearchPanel(s => s.Visible(false))
                        .ShowBorders(true)
                        .ShowColumnHeaders(true)
                        .ShowRowLines(false).ShowColumnLines(false)
                        .WordWrapEnabled(true)
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

            gridView.Fields(field =>
            {
                field.ClearFields();

                field.AddField(f => f.Image);
                field.AddField(f => f.Code);
                field.AddField(f => f.Name);
                field.AddField(f => f.Division);
                field.AddField(f => f.Range);
                field.AddField(f => f.Action);
                field.AddField(f => f.DateTime);
                //field.AddField(f => f.EmployeeType);
            });

            gridView.OnLoadData += GridView_OnLoadData;

            var btnAddNew = gridView.AddButton("Add New");
            btnAddNew.OnClick += BtnAddNew_OnClick;
            popupAddNew = this.AddPopupView();
            popupAddNew.SetId("popupAddNew");
            popupAddNew.Title = "Add New";
            popupAddNew.AddCardView<Employee>().OnSubmit += DetectionLogsAddNewController_OnSubmit;

            var btnMapTo = gridView.AddButton("Map To");
            btnMapTo.OnClick += BtnMapTo_OnClick;
            popupMapTo = this.AddPopupView();
            popupMapTo.SetId("popupMapTo");
            popupMapTo.Title = "Map To";
            popupMapTo.AddCardView<MapTo>().OnSubmit += DetectionLogsMapToController_OnSubmit;

        }

       

        private async Task CardView_OnSubmit(DetectionLogsFilterData model, IFormFileCollection files)
        {
            model.GetDates();
        }

        private async Task<LoadResult> GridView_OnLoadData(DataSourceLoadOptions loadOptions, string? parentId)
        {
            var TenantId =   AppStudio.Framework.Core.CoreServices.CurrentUser.GetTenantId();

            DateTime fromDate =
                (cardView.Data.FromDate == DateTime.MinValue)
                ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"))
                : DateTime.Parse(cardView.Data.FromDate.ToString("yyyy-MM-dd"));
            DateTime toDate =
                (cardView.Data.ToDate == DateTime.MinValue)
                ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"))
                : DateTime.Parse(cardView.Data.ToDate.ToString("yyyy-MM-dd"));
            string personType = cardView.Data.GetPersonType();
            string cameraName = cardView.Data.GetCameraName();


            //string sql =
            //       $"SELECT id AS Id" +
            //       $"     , faceImage AS FaceImage" +
            //       $"     , employeeId AS EmployeeId" +
            //       $"     , IsNull(code, '') AS Code" +
            //       $"     , IsNull(name, '') AS Name" +
            //       $"     , action AS Action" +
            //       $"     , cameraName AS CameraName" +
            //       $"     , CAST(timeStamp AS TIME) AS Time" +
            //       $"     , CAST(dateTime AS DATE) AS Date" +
            //       $"     , (CASE WHEN employeeId = '00000000-0000-0000-0000-000000000000' THEN 'Unidentified' ELSE 'Employee' END) as PersonType" +
            //       $"  FROM attendanceindex" +
            //       $" WHERE TenantId = '{TenantId}'";
            //if (!string.IsNullOrEmpty(cameraName)) sql += $"   AND cameraName = '{cameraName}'";
            //sql += $"   AND (dateTime >= '{fromDate}' AND dateTime <= '{toDate}')" +
            //       $" ORDER BY dateTime DESC, timeStamp DESC";


            var peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();
            var query = peopleDb.AttendanceLogs
                .Where(ai => ai.TenantId == TenantId
                    && ai.DateTime.Date >= fromDate.Date
                    && ai.DateTime.Date <= toDate.Date
                    && ai.Image != ""
                ).OrderByDescending( m => m.DateTime);
            //if (!string.IsNullOrEmpty(cameraName)) query = query.Where(ai => ai.CameraName == cameraName);
            var results = query
                .OrderByDescending(ai => ai.DateTime)
                //.ThenByDescending(ai => ai.TimeStamp)
                .Select(ai => new AttendanceIndex
                {
                    Id = ai.Id,
                    Image = ai.Image,
                    EmployeeId = ai.EmployeeId,
                    Code = string.IsNullOrEmpty(ai.Code) ? "" : ai.Code,
                    Name = string.IsNullOrEmpty(ai.Name) ? "" : ai.Name,
                    Action = ai.Action,
                    //CameraName = ai.CameraName,
                    //Time = ai.TimeStamp.ToString("HH:mm:ss"),
                    Date = ai.DateTime.ToString("yyyy-MM-dd"),
                    DateTime= ai.DateTime,
                    Division = ai.Division,
                    Range = ai.Range,
                    EmployeeType =ai.EmployeeType,
                    PersonType = (ai.EmployeeId == Guid.Empty || ai.EmployeeId == null) ? "Unidentified" : "Employee"
                }).OrderByDescending(m => m.DateTime)
                .ToList();
            if (personType != "All") results = results.Where(m => m.PersonType.Equals(personType)).OrderByDescending(m => m.DateTime).ToList();
            foreach (var result in results)
            {
                result.Date = DateTime.Parse(result.Date).ToString("dd-MMM-yy");
                //result.Time = TimeOnly.Parse(result.Time.Substring(0, 8)).AddHours(5).AddMinutes(30).ToString("HH:mm:ss");
                //result.Time = TimeOnly.Parse(result.Time).ToString("HH:mm:ss");

            }
            
            return DataSourceLoader.Load(results.OrderBy(m => m.DateTime), loadOptions);
        }

        private void BtnAddNew_OnClick(ButtonItem obj)
        {
            attendanceLogId = obj.RecordId;
            popupAddNew.Show();
        }

        private void BtnMapTo_OnClick(ButtonItem obj)
        {
            attendanceLogId = obj.RecordId;
            popupMapTo.Show();
        }

        private async Task DetectionLogsAddNewController_OnSubmit(Employee model, IFormFileCollection files)
        {
            //  --------------------------------------------------
            //  Create new Employee
            //  --------------------------------------------------
            var result = await Entity.InsertAsync(model);

            //  --------------------------------------------------
            //  Get Image Stream
            //  --------------------------------------------------
            IFileStorageService fileStorage = CoreServices.FileStorageService;
            string path = Path.Combine(nameof(AttendanceLog), nameof(AttendanceLog.Image));

            var stream = await fileStorage.GetFileAsync("documents", path, attendanceLogId.ToString());
            //var stream = await fileStorage.GetFileAsync("documents", $"{nameof(AttendanceLog)}\\{nameof(AttendanceLog.Image)}", attendanceLogId.ToString());

            //  --------------------------------------------------
            //  Assign Employee details to AttendanceLog
            //  --------------------------------------------------
            var attendanceLogRep = CoreServices.GetRepository<AttendanceLog>();
            var employee= CoreServices.GetRepository<Employee>();
            Guid id = Guid.Parse(attendanceLogId);
            var attendanceLog = await attendanceLogRep.FindByIdAsync(id);
            attendanceLog.TenantId = model.TenantId;
            attendanceLog.UserId = model.Id;
            attendanceLog.EmployeeId = model.Id;
            attendanceLog.Name = model.Name;
            attendanceLog.Code = model.Code;
            attendanceLog.Gender = model.Gender.ToString();
           
            if (model.EmployeeTypeId != null)
            {
                var type = CoreServices.GetRepository<EmployeeType>();
                var empType = await type.FindByIdAsync(model.EmployeeTypeId);
                attendanceLog.EmployeeTypeId = empType.Id;
                attendanceLog.EmployeeType = empType.Name;
            }
            if (model.CircleId != null)
            {
                var cir = CoreServices.GetRepository<Circle>();
                var empcir = await cir.FindByIdAsync(model.CircleId);
                attendanceLog.CircleId = empcir.Id;
                attendanceLog.Circle = empcir.Name;
            }
            if(model.DivisionId != null)
            {
                var div = CoreServices.GetRepository<Division>();
                var empdiv = await div.FindByIdAsync(model.DivisionId);
                attendanceLog.DivisionId = empdiv.Id;
                attendanceLog.Division = empdiv.Name;
            }
            if(model.RangeId != null)
            {
                var range = CoreServices.GetRepository<Range>();
                var emprange = await range.FindByIdAsync(model.RangeId);
                attendanceLog.RangeId = emprange.Id;
                attendanceLog.Range = emprange.Name;
            }
            if (model.SectionId != null)
            {
                var section = CoreServices.GetRepository<Section>();
                var empsection = await section.FindByIdAsync(model.SectionId);
                attendanceLog.SectionId = empsection.Id;
                attendanceLog.Section = empsection.Name;
            }
            if(model.BeatId != null)
            {
                var beat = CoreServices.GetRepository<Beat>();
                var empbeat = await beat.FindByIdAsync(model.BeatId);
                attendanceLog.BeatId = empbeat.Id;
                attendanceLog.Beat = empbeat.Name;
                if (attendanceLog.Latitude.HasValue && attendanceLog.Longitude.HasValue)
                {
                    var reader = new WKTReader();
                    Geometry geometry = (Geometry)reader.Read(empbeat.Polygon);
                    Polygon poly = geometry as Polygon;
                    var geometryFactory = new GeometryFactory();


                    var polygon = geometryFactory.CreatePolygon(poly.Coordinates);
                    var point = geometryFactory.CreatePoint(new Coordinate((double)attendanceLog.Longitude, (double)attendanceLog.Latitude));
                    double distanceDegrees = point.Distance(polygon);

                    const double earthRadiusMeters = 6371000;
                    double meters = distanceDegrees * (Math.PI / 180) * earthRadiusMeters;
                    double roundedMeters = Math.Round(meters);

                    string message = string.Empty;
                    if (roundedMeters > 0)
                    {

                        message = "you are " + roundedMeters + " meters away from the " + empbeat.Name + " area";
                    }
                    else
                    {
                        message = "you are in the " + empbeat.Name + " area";
                    }
                    attendanceLog.Message = message;
                }
            }
            if (model.ShiftId != null)
            {
                var shift = CoreServices.GetRepository<Shift>();
                var empshift = await shift.FindByIdAsync(model.ShiftId);
                attendanceLog.ShiftId = empshift.Id;
                attendanceLog.Shift = empshift.Name;
                attendanceLog.StartTime = empshift.StartTime;
                attendanceLog.EndTime = empshift.EndTime;
            }
            else
            {
                attendanceLog.StartTime = new TimeOnly(0, 0);
                attendanceLog.EndTime = new TimeOnly(0, 0);
            }            
            // Fix for CS1503 errors
           

            await attendanceLogRep.UpdateAsync(attendanceLog);

            if (stream != null)
            {
                //  --------------------------------------------------
                //  Create new image record for Employee
                //  --------------------------------------------------
                EmployeeImage employeeImage = new EmployeeImage();
                employeeImage.Image = "Photo.PNG";
                employeeImage.EmployeeId = model.Id;
                employeeImage.TenantId = model.TenantId;
                employeeImage.Embedding = attendanceLog.Embedding;
                //  ==================================================

                //  --------------------------------------------------
                //  Create new image for Employee
                //  --------------------------------------------------
                var empImgRep = CoreServices.GetRepository<EmployeeImage>();
                await empImgRep.InsertAsync(employeeImage);
                string pathiImg = Path.Combine(nameof(EmployeeImage), nameof(EmployeeImage.Image));

                await fileStorage.SaveFileAsync("documents", pathiImg, employeeImage.Id.ToString(), MimeHelper.GetMimeType("jpg"), stream);
                //  ==================================================
            }
            UiAction.ClosePopup();
        }
       
        private async Task DetectionLogsMapToController_OnSubmit(MapTo model, IFormFileCollection files)
        {
            //  --------------------------------------------------
            //  Fetch Employee Details
            //  --------------------------------------------------
            var employeeRep = CoreServices.GetRepository<Employee>();
            var employee = await employeeRep.FindByIdAsync(model.EmployeeId);
            //  ==================================================

            //  --------------------------------------------------
            //  Get Image Stream
            //  --------------------------------------------------
            IFileStorageService fileStorage = CoreServices.FileStorageService;
            //var stream = await fileStorage.GetFileAsync("documents", $"{nameof(AttendanceLog)}\\{nameof(AttendanceLog.Image)}", attendanceLogId.ToString());
            string path = Path.Combine(nameof(AttendanceLog), nameof(AttendanceLog.Image));

            var stream = await fileStorage.GetFileAsync("documents", path, attendanceLogId.ToString());
            //  ==================================================

            //  --------------------------------------------------
            //  Assign Employee details to AttendanceLog
            //  --------------------------------------------------
            var attendanceLogRep = CoreServices.GetRepository<AttendanceLog>();
            Guid id = Guid.Parse(attendanceLogId);
            var attendanceLog = await attendanceLogRep.FindByIdAsync(id);
            attendanceLog.TenantId = employee.TenantId;
            attendanceLog.UserId = employee.Id;
            attendanceLog.EmployeeId = employee.Id;
            attendanceLog.Name = employee.Name;
            attendanceLog.Code = employee.Code;
            attendanceLog.Gender = employee.Gender.ToString();

            if (employee.EmployeeTypeId != null)
            {
                var type = CoreServices.GetRepository<EmployeeType>();
                var empType = await type.FindByIdAsync(employee.EmployeeTypeId);
                attendanceLog.EmployeeTypeId = empType.Id;
                attendanceLog.EmployeeType = empType.Name;
            }
            if (employee.CircleId != null)
            {
                var cir = CoreServices.GetRepository<Circle>();
                var empcir = await cir.FindByIdAsync(employee.CircleId);
                attendanceLog.CircleId = empcir.Id;
                attendanceLog.Circle = empcir.Name;
            }
            if (employee.DivisionId != null)
            {
                var div = CoreServices.GetRepository<Division>();
                var empdiv = await div.FindByIdAsync(employee.DivisionId);
                attendanceLog.DivisionId = empdiv.Id;
                attendanceLog.Division = empdiv.Name;
            }
            if (employee.RangeId != null)
            {
                var range = CoreServices.GetRepository<Range>();
                var emprange = await range.FindByIdAsync(employee.RangeId);
                attendanceLog.RangeId = emprange.Id;
                attendanceLog.Range = emprange.Name;
            }
            if (employee.SectionId != null)
            {
                var section = CoreServices.GetRepository<Section>();
                var empsection = await section.FindByIdAsync(employee.SectionId);
                attendanceLog.SectionId = empsection.Id;
                attendanceLog.Section = empsection.Name;
            }
            if (employee.BeatId != null)
            {
                var beat = CoreServices.GetRepository<Beat>();
                var empbeat = await beat.FindByIdAsync(employee.BeatId);
                attendanceLog.BeatId = empbeat.Id;
                attendanceLog.Beat = empbeat.Name;
                if (attendanceLog.Latitude.HasValue && attendanceLog.Longitude.HasValue)
                {
                    var reader = new WKTReader();
                    Geometry geometry = (Geometry)reader.Read(empbeat.Polygon);
                    Polygon poly = geometry as Polygon;
                    var geometryFactory = new GeometryFactory();


                    var polygon = geometryFactory.CreatePolygon(poly.Coordinates);
                    var point = geometryFactory.CreatePoint(new Coordinate((double)attendanceLog.Longitude, (double)attendanceLog.Latitude));
                    double distanceDegrees = point.Distance(polygon);

                    const double earthRadiusMeters = 6371000;
                    double meters = distanceDegrees * (Math.PI / 180) * earthRadiusMeters;
                    double roundedMeters = Math.Round(meters);

                    string message = string.Empty;
                    if (roundedMeters > 0)
                    {

                        message = "you are " + roundedMeters + " meters away from the " + empbeat.Name + " area";
                    }
                    else
                    {
                        message = "you are in the " + empbeat.Name + " area";
                    }
                    attendanceLog.Message = message;
                }
            }
            if (employee.ShiftId != null)
            {
                var shift = CoreServices.GetRepository<Shift>();
                var empshift = await shift.FindByIdAsync(employee.ShiftId);
                attendanceLog.ShiftId = empshift.Id;
                attendanceLog.Shift = empshift.Name;
                attendanceLog.StartTime = empshift.StartTime;
                attendanceLog.EndTime = empshift.EndTime;
            }
            else
            {
                attendanceLog.StartTime = new TimeOnly(0, 0);
                attendanceLog.EndTime = new TimeOnly(0, 0);
            }
            await attendanceLogRep.UpdateAsync(attendanceLog);
            //  ==================================================

            //  --------------------------------------------------
            //  Create new image record for Employee
            //  --------------------------------------------------
            EmployeeImage employeeImage = new EmployeeImage();
            employeeImage.Image = "Photo.PNG";
            employeeImage.EmployeeId = employee.Id;
            employeeImage.TenantId = employee.TenantId;
            employeeImage.Embedding = attendanceLog.Embedding;
            //  ==================================================

            //  --------------------------------------------------
            //  Create new image for Employee
            //  --------------------------------------------------
            var empImgRep = CoreServices.GetRepository<EmployeeImage>();
            await empImgRep.InsertAsync(employeeImage);
            //await fileStorage.SaveFileAsync("documents", $"{nameof(EmployeeImage)}\\{nameof(EmployeeImage.Image)}", employeeImage.Id.ToString(), MimeHelper.GetMimeType("jpg"), stream);
            string pathiImg = Path.Combine(nameof(EmployeeImage), nameof(EmployeeImage.Image));

            await fileStorage.SaveFileAsync("documents", pathiImg, employeeImage.Id.ToString(), MimeHelper.GetMimeType("jpg"), stream);
            //  ==================================================

            UiAction.ClosePopup();
        }
    }

    public class DetectionLogsFilterData
    {
        public EmployeePersonType PersonType { get; set; } = EmployeePersonType.All;

        public Guid CameraId { get; set; } = Guid.Empty;

        public DetetionLogsPeriod Period { get; set; } = DetetionLogsPeriod.Today;

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public DetectionLogsFilterData()
        {
            FromDate = DateTime.Today;
            ToDate = DateTime.Today;
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
            DateTime fromDate = DateTime.Today;
            DateTime toDate = DateTime.Today;

            switch (Period)
            {
                case DetetionLogsPeriod.Today:
                    fromDate = DateTime.Today;
                    toDate = DateTime.Today;
                    break;
                case DetetionLogsPeriod.ThisWeek:
                    fromDate = fromDate.AddDays(-(int)fromDate.DayOfWeek + (int)System.DayOfWeek.Monday);
                    toDate = fromDate.AddDays(6);
                    break;
                case DetetionLogsPeriod.ThisMonth:
                    fromDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    toDate = fromDate.AddMonths(1).AddDays(-1);
                    break;
                case DetetionLogsPeriod.LastMonth:
                    fromDate = new DateTime(DateTime.Now.AddMonths(-1).Year, DateTime.Now.AddMonths(-1).Month, 1);
                    toDate = fromDate.AddMonths(1).AddDays(-1);
                    break;
                case DetetionLogsPeriod.CustomDateRange:
                    fromDate = DateTime.Parse(FromDate.ToString());
                    toDate = DateTime.Parse(ToDate.ToString());
                    break;
                default:
                    break;
            }

            FromDate = DateTime.Parse(fromDate.ToString("yyyy-MM-dd"));
            ToDate = DateTime.Parse(toDate.ToString("yyyy-MM-dd"));
        }
    }

    public class AttendanceIndex
    {
        [Key]
        public Guid? Id { get; set; }

        [MaxLength(250)]
        [DataType(DataType.ImageUrl)]
        public string Image { get; set; }

        public Guid? EmployeeId { get; set; }

        public string? Code { get; set; }

        public string? Name { get; set; }

        public string? Action { get; set; }

        public string? CameraName { get; set; }


        public string? Time { get; set; }

        public string? Date { get; set; }

        [UIControl(ControlType.DateTimePicker)]
        public DateTime DateTime { get; set; }

        public string? Division { get; set; }
        public string Range { get; set; }

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
