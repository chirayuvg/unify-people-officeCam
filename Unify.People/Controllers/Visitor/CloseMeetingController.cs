using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using Microsoft.AspNetCore.Authorization;
using Unify.Data;
using Unify.People.Services;

namespace Unify.People.Controllers.Visitors
{
    [Authorize("CookiePolicy")]
    public class CloseMeetingController : PageController
    {
        private readonly AppointmentServices _appointmentServices;

        public CardView<CloseMeetingModel> cardView;

        public CloseMeetingController(AppointmentServices appointmentServices)
        {
            _appointmentServices = appointmentServices;
        }

        public override void Init()
        {
            this.Page.SetHeight("200px");
            this.Page.SetWidth("400px");

            var appointmentId = this.Page.History.Parameters[0].Value;
            var peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();
            var appointment = peopleDb.Appointments
                .Where(m => m.Id == Guid.Parse(appointmentId))
                .FirstOrDefault();

            cardView = this.AddCardView<CloseMeetingModel>();
            cardView.Data.AppointmentId = Guid.Parse(appointmentId);
            cardView.Data.StartTime = appointment.StartTime;
            var now = DateTime.Now;
            cardView.Data.EndTime = TimeOnly.Parse(now.Hour.ToString() + ":" + now.Minute.ToString());
            cardView.GetFieldControl(m => m.StartTime).IsReadOnly = true;
            cardView.OnSubmit += CardViewCloseMeeting_OnSubmit;
        }

        private async Task CardViewCloseMeeting_OnSubmit(CloseMeetingModel model, IFormFileCollection files)
        {
            await _appointmentServices.CloseMeeting(model);
            UiAction.ClosePopup();
        }
    }
}
