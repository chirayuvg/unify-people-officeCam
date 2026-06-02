using AppStudio.Enterprise.Data;
using AppStudio.Framework.Core;
using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Unify.Data;
using Unify.Data.SQL;
using Unify.People.Services;

namespace Unify.People.Controllers.Visitors
{
    public class AppointmentController : PageController
    {
        private readonly EmployeeService _employeeService;

        private CardView<AppointmentFilterData> cardView;
        private FormView<Appointment> formView;
        private GridView<Appointment> gridView;

        [ViewState]
        public string appointmentId { get; set; }

        public AppointmentController(EmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        public override void Init()
        {
            this.Page.SetTitle("Appointment List");

            cardView = this.AddCardView<AppointmentFilterData>().SetControlStyle(CardViewStyle.Filter);
            cardView.ControlStyle = CardViewStyle.Filter;
            cardView.SubmitOnDataChange = true;
            cardView.LabelPosition = LabelPosition.Left;
            cardView.OnSubmit += CardView_OnSubmit;

            formView = this.AddFormView<Appointment>();
            formView.SetLayout(FormLayout.Tab);
            formView.GetFieldControl(m => m.StartTime).IsVisible = false;
            formView.GetFieldControl(m => m.EndTime).IsVisible = false;
            formView.GetFieldControl("Status").IsVisible = false;
            formView.GetFieldControl("MeetingDate").IsVisible = false;
            formView.OnBeforeInsert += FormView_OnBeforeInsert;
            formView.OnBeforeUpdate += FormView_OnBeforeUpdate;

            gridView = this.AddGridView<Appointment>();
            gridView
                .SetFormView(formView)
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

                field.AddField(f => f.PrimaryVisitor).ColumnWidth=100;
                field.AddField(f => f.MobileNo).ColumnWidth = 100;
                field.AddField(f => f.Company).ColumnWidth = 100;
                field.AddField(f => f.EmployeeId).ColumnWidth = 100;
                field.AddField(f => f.MeetingPurposeId).ColumnWidth = 100;
                field.AddField(f => f.MeetingPlaceId).ColumnWidth = 100;
                field.AddField(f => f.AppointmentDate).ColumnWidth = 100;
                field.AddField(f => f.AppointmentTime).ColumnWidth = 100;
                field.AddField(f => f.Status).ColumnWidth = 100;
            });
            gridView.SetDefaultSort("PrimaryVisitor", false);
            gridView.OnLoadData += GridView_OnLoadData;

            var btnCheckIn = gridView.AddButton("CheckIn");
            btnCheckIn.OnClick += BtnCheckIn_OnClick;
        }

        public override Task ProcessAsync()
        {
            formView.OnBeforeInsert += FormView_OnBeforeInsert;
            formView.OnBeforeUpdate += FormView_OnBeforeUpdate;

            return base.ProcessAsync();
        }
        private Task<Result> FormView_OnBeforeInsert(Appointment entity, IFormFileCollection files, string parentId)
        {
            var result = new Result();

            try
            {
                //  Validate Appointment Date
                if (entity.AppointmentDate < DateOnly.FromDateTime(DateTime.Now))
                {
                    UiAction.AddJsCode("alert('Appointment date must be greater than or equal to the Current Date');");
                    result.AddError("AppointmentId", "Appointment date must be greater than or equal to the Current Date");
                }

                //  Validate Appointment Time
                if ((entity.AppointmentDate == DateOnly.FromDateTime(DateTime.Now)) && (entity.AppointmentTime < TimeOnly.FromDateTime(DateTime.Now)))
                {
                    UiAction.AddJsCode("alert('Appointment Time must be greater than or equal to the Current Time');");
                    result.AddError("AppointmentId", "Appointment Time must be greater than or equal to the Current Time");
                }
            }
            catch (Exception ex)
            {
                var error = ex.Message;
            }

            return Task.FromResult(result);
        }

        private Task<Result> FormView_OnBeforeUpdate(Appointment entity, IFormFileCollection files, string parentId)
        {
            var result = new Result();

            try
            {
                //  Validate Appointment Date
                if (entity.AppointmentDate < DateOnly.FromDateTime(DateTime.Now))
                {
                    UiAction.AddJsCode("alert('Appointment date must be greater than or equal to the Current Date');");
                    result.AddError("AppointmentId", "Appointment date must be greater than or equal to the Current Date");
                }

                //  Validate Appointment Time
                if ((entity.AppointmentDate == DateOnly.FromDateTime(DateTime.Now)) && (entity.AppointmentTime < TimeOnly.FromDateTime(DateTime.Now)))
                {
                    UiAction.AddJsCode("alert('Appointment Time must be greater than or equal to the Current Time');");
                    result.AddError("AppointmentId", "Appointment Time must be greater than or equal to the Current Time");
                }
            }
            catch (Exception ex)
            {
                var error = ex.Message;
            }

            return Task.FromResult(result);
        }

        private async Task CardView_OnSubmit(AppointmentFilterData model, IFormFileCollection files)
        {
            model.GetAppointmentPeriod();
        }

        private async Task<DevExtreme.AspNet.Data.ResponseModel.LoadResult> GridView_OnLoadData(DataSourceLoadOptions loadOptions, string? parentId)
        {
            var TenantId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetTenantId();
            var UserId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetUserId();

            string appointmentPeriod = cardView.Data.AppointmentPeriod.ToString();
            var peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();
            List<Appointment> appointments = new List<Appointment>();

            DateOnly ToDayDateOnly = DateOnly.Parse(DateTime.Now.ToString("yyyy-MM-dd"));

            TenantUser tenantUser = await Entity.FindByIdAsync<TenantUser>(UserId);

            if (appointmentPeriod == "All")
            {
                if (tenantUser.Roles.Any(r => (r == "Super Admin") || (r == "Admin")))
                {
                    appointments = peopleDb.Appointments
                        .Where(app => app.TenantId == TenantId
                            && (app.Status != "Closed" && app.Status != "In Meeting")
                        )
                        .OrderBy(app => app.AppointmentDate).ThenBy(app => app.AppointmentTime)
                        .ToList();
                }
                else
                {
                    var employee = await _employeeService.GetEmployeeByEmailAsync(TenantId, tenantUser.Email);

                    appointments = peopleDb.Appointments
                        .Where(app => app.TenantId == TenantId
                            && (app.Status != "Closed" && app.Status != "In Meeting")
                            && (app.EmployeeId == employee.Id)
                        )
                        .OrderBy(app => app.AppointmentDate).ThenBy(app => app.AppointmentTime)
                        .ToList();
                }

                foreach (Appointment appointment in appointments)
                {
                    if (appointment.AppointmentDate < ToDayDateOnly) appointment.Status = "Not Conducted";
                }
            }
            else if (appointmentPeriod == "NotConducted")
            {
                if (tenantUser.Roles.Any(r => (r == "Super Admin") || (r == "Admin")))
                {
                    appointments = peopleDb.Appointments
                        .Where(app => app.TenantId == TenantId
                            && app.Status == "Pending"
                            && app.AppointmentDate < ToDayDateOnly
                        )
                        .OrderBy(app => app.AppointmentDate).ThenBy(app => app.AppointmentTime)
                        .ToList();
                }
                else
                {
                    var employee = await _employeeService.GetEmployeeByEmailAsync(TenantId, tenantUser.Email);

                    appointments = peopleDb.Appointments
                        .Where(app => app.TenantId == TenantId
                            && app.Status == "Pending"
                            && app.AppointmentDate < ToDayDateOnly
                            && (app.EmployeeId == employee.Id)
                        )
                        .OrderBy(app => app.AppointmentDate).ThenBy(app => app.AppointmentTime)
                        .ToList();
                }

                foreach (Appointment appointment in appointments)
                {
                    appointment.Status = "Not Conducted";
                }
            }
            else if (appointmentPeriod == "Pending")
            {
                if (tenantUser.Roles.Any(r => (r == "Super Admin") || (r == "Admin")))
                {
                    appointments = peopleDb.Appointments
                        .Where(app => app.TenantId == TenantId
                            && app.Status == "Pending"
                            && app.AppointmentDate >= ToDayDateOnly
                        )
                        .OrderBy(app => app.AppointmentDate).ThenBy(app => app.AppointmentTime)
                        .ToList();
                }
                else
                {
                    var employee = await _employeeService.GetEmployeeByEmailAsync(TenantId, tenantUser.Email);

                    appointments = peopleDb.Appointments
                        .Where(app => app.TenantId == TenantId
                            && app.Status == "Pending"
                            && app.AppointmentDate >= ToDayDateOnly
                            && (app.EmployeeId == employee.Id)
                        )
                        .OrderBy(app => app.AppointmentDate).ThenBy(app => app.AppointmentTime)
                        .ToList();
                }
            }
            else if (appointmentPeriod == "Today")
            {
                if (tenantUser.Roles.Any(r => (r == "Super Admin") || (r == "Admin")))
                {
                    appointments = peopleDb.Appointments
                        .Where(app => app.TenantId == TenantId
                            && app.Status == "Pending"
                            && app.AppointmentDate == ToDayDateOnly
                        )
                        .OrderBy(app => app.AppointmentDate).ThenBy(app => app.AppointmentTime)
                        .ToList();
                }
                else
                {
                    var employee = await _employeeService.GetEmployeeByEmailAsync(TenantId, tenantUser.Email);

                    appointments = peopleDb.Appointments
                        .Where(app => app.TenantId == TenantId
                            && app.Status == "Pending"
                            && app.AppointmentDate == ToDayDateOnly
                            && (app.EmployeeId == employee.Id)
                        )
                        .OrderBy(app => app.AppointmentDate).ThenBy(app => app.AppointmentTime)
                        .ToList();
                }
            }

            return DataSourceLoader.Load(appointments.OrderByDescending(m => m.AppointmentDate).ThenByDescending(m => m.AppointmentTime), loadOptions);
        }

        private void BtnCheckIn_OnClick(ButtonItem obj)
        {
            appointmentId = obj.RecordId;
            UiAction.ShowPage("Index", "VisitorLog", new { AppointmentId = appointmentId });
        }
    }

    public enum AppointmentPeriods
    {
        All,
        NotConducted,
        Pending,
        Today
    }

    public class AppointmentFilterData
    {
        public AppointmentPeriods AppointmentPeriod { get; set; } = AppointmentPeriods.Today;

        public string GetAppointmentPeriod()
        {
            return AppointmentPeriod switch
            {
                AppointmentPeriods.All => "All",
                AppointmentPeriods.NotConducted => "Not Conducted",
                AppointmentPeriods.Pending => "Pending",
                AppointmentPeriods.Today => "Today",
                _ => throw new NotImplementedException()
            };
        }
    }
}
