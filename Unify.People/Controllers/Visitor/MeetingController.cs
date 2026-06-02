using AppStudio.Enterprise.Data;
using AppStudio.Framework.Core;
using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using System.ComponentModel.DataAnnotations;
using Unify.Data;
using Unify.Data.SQL;
using Unify.People.Services;

namespace Unify.People.Controllers.Visitors
{
    public class MeetingController : PageController
    {
        private readonly AppointmentServices _appointmentServices;
        private readonly EmployeeService _employeeService;

        private CardView<MeetingFilterData> cardViewMeetingFilterData;
        private GridView<Appointment> gridView;

        public MeetingController(AppointmentServices appointmentServices, EmployeeService employeeService)
        {
            _appointmentServices = appointmentServices;
            _employeeService = employeeService;
        }

        public override void Init()
        {
            CoreServices.Notification.SendEmailAsync("", "", "");

            this.Page.SetTitle("Meeting List");

            cardViewMeetingFilterData = this.AddCardView<MeetingFilterData>().SetControlStyle(CardViewStyle.Filter);
            cardViewMeetingFilterData.ControlStyle = CardViewStyle.Filter;
            cardViewMeetingFilterData.SubmitOnDataChange = true;
            cardViewMeetingFilterData.LabelPosition = LabelPosition.Left;
            cardViewMeetingFilterData.OnSubmit += CardViewMeetingFilterData_OnSubmit;

            gridView = this.AddGridView<Appointment>();
            gridView
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

                field.AddField(f => f.PrimaryVisitor).ColumnWidth = 100;
                field.AddField(f => f.MobileNo).ColumnWidth = 100;
                field.AddField(f => f.Company).ColumnWidth = 100;
                field.AddField(f => f.EmployeeId).ColumnWidth = 100;
                field.AddField(f => f.AppointmentDate).ColumnWidth = 100;
                field.AddField(f => f.Status).ColumnWidth = 100;
                field.AddField(f => f.MeetingDate).ColumnWidth = 100;
                field.AddField(f => f.StartTime).ColumnWidth = 100;
                field.AddField(f => f.EndTime).ColumnWidth = 100;
            });
            gridView.SetDefaultSort("PrimaryVisitor", false);
            gridView.OnLoadData += GridView_OnLoadData;

            var btnCloseMeeting = gridView.AddButton("Close Meeting");
            btnCloseMeeting.OnClick += BtnCloseMeeting_OnClick;
        }

        public override Task ProcessAsync()
        {
            return base.ProcessAsync();
        }

        private async Task CardViewMeetingFilterData_OnSubmit(MeetingFilterData model, IFormFileCollection files)
        {
            model.GetMeetingPeriod();
        }

        private async Task<DevExtreme.AspNet.Data.ResponseModel.LoadResult> GridView_OnLoadData(DataSourceLoadOptions loadOptions, string? parentId)
        {
            var TenantId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetTenantId();
            var UserId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetUserId();

            string meetingPeriod = cardViewMeetingFilterData.Data.MeetingPeriod.ToString();
            var peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();
            List<Appointment> appointments = new List<Appointment>();

            DateOnly ToDayDateOnly = DateOnly.Parse(DateTime.Now.ToString("yyyy-MM-dd"));

            TenantUser tenantUser = await Entity.FindByIdAsync<TenantUser>(UserId);

            if (meetingPeriod == "All")
            {
                if (tenantUser.Roles.Any(r => (r == "Super Admin") || (r == "Admin")))
                {
                    appointments = peopleDb.Appointments
                        .Where(app => app.TenantId == TenantId
                            && (app.Status == "Closed" || app.Status == "In Meeting")
                        )
                        .OrderBy(app => app.AppointmentDate).ThenBy(app => app.AppointmentTime)
                        .ToList();
                }
                else
                {
                    var employee = await _employeeService.GetEmployeeByEmailAsync(TenantId, tenantUser.Email);

                    appointments = peopleDb.Appointments
                        .Where(app => app.TenantId == TenantId
                            && (app.Status == "Closed" || app.Status == "In Meeting")
                            && (app.EmployeeId == employee.Id)
                        )
                        .OrderBy(app => app.AppointmentDate).ThenBy(app => app.AppointmentTime)
                        .ToList();
                }
            }
            else if (meetingPeriod == "Closed")
            {
                if (tenantUser.Roles.Any(r => (r == "Super Admin") || (r == "Admin")))
                {
                    appointments = peopleDb.Appointments
                        .Where(app => app.TenantId == TenantId
                            && app.Status == "Closed"
                        )
                        .OrderBy(app => app.AppointmentDate).ThenBy(app => app.AppointmentTime)
                        .ToList();
                }
                else
                {
                    var employee = await _employeeService.GetEmployeeByEmailAsync(TenantId, tenantUser.Email);

                    appointments = peopleDb.Appointments
                        .Where(app => app.TenantId == TenantId
                            && app.Status == "Closed"
                            && (app.EmployeeId == employee.Id)
                        )
                        .OrderBy(app => app.AppointmentDate).ThenBy(app => app.AppointmentTime)
                        .ToList();
                }
            }
            else if (meetingPeriod == "InMeeting")
            {
                if (tenantUser.Roles.Any(r => (r == "Super Admin") || (r == "Admin")))
                {
                    appointments = peopleDb.Appointments
                        .Where(app => app.TenantId == TenantId
                            && app.Status == "In Meeting"
                        )
                        .OrderBy(app => app.AppointmentDate).ThenBy(app => app.AppointmentTime)
                        .ToList();
                }
                else
                {
                    var employee = await _employeeService.GetEmployeeByEmailAsync(TenantId, tenantUser.Email);

                    appointments = peopleDb.Appointments
                        .Where(app => app.TenantId == TenantId
                            && app.Status == "In Meeting"
                            && (app.EmployeeId == employee.Id)
                        )
                        .OrderBy(app => app.AppointmentDate).ThenBy(app => app.AppointmentTime)
                        .ToList();
                }
            }

            return DataSourceLoader.Load(appointments.OrderByDescending(m => m.AppointmentDate).ThenByDescending(m => m.AppointmentTime), loadOptions);
        }

        private void BtnCloseMeeting_OnClick(ButtonItem obj)
        {
            UiAction.ShowPopup("Close Meeting", "CloseMeeting", "Index", new { AppointmentId = obj.RecordId });
        }
    }

    public enum MeetingPeriods
    {
        All,
        Closed,
        InMeeting
    }

    public class MeetingFilterData
    {
        public MeetingPeriods MeetingPeriod { get; set; } = MeetingPeriods.InMeeting;

        public string GetMeetingPeriod()
        {
            return MeetingPeriod switch
            {
                MeetingPeriods.All => "All",
                MeetingPeriods.Closed => "Closed",
                MeetingPeriods.InMeeting => "In Meeting",
                _ => throw new NotImplementedException()
            };
        }
    }

    public class CloseMeetingModel : IValidatableObject
    {
        public Guid AppointmentId { get; set; }

        [Required]
        public TimeOnly? StartTime { get; set; }

        [Required]
        public TimeOnly? EndTime { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (StartTime.HasValue && EndTime.HasValue)
            {
                if (EndTime <= StartTime)
                {
                    yield return new ValidationResult(
                        "End time must be greater than start time.", new[] { nameof(EndTime) }
                    );
                }
            }
        }
    }
}
