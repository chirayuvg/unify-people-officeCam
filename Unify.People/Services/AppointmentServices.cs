using AppStudio.Framework.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DynamicLinq;
using Unify.Data;
using Unify.Data.SQL;
using Unify.People.Controllers.Visitors;

namespace Unify.People.Services
{
    public class AppointmentServices
    {
        public AppointmentServices(IDbContextFactory<PeopleDb> dbContextFactory)
        {

        }

        public async Task StartInMeeting(Guid AppointmentId)
        {
            try
            {
                TimeOnly timeOnly = TimeOnly.FromDateTime(DateTime.Now);

                //  Update StartTime in Appointment
                Appointment appointment = await Entity.Query<Appointment>().FirstOrDefaultAsync(m => m.Id == AppointmentId);
                appointment.Status = "In Meeting";
                appointment.MeetingDate = DateOnly.FromDateTime(DateTime.Now);
                appointment.StartTime = new TimeOnly(timeOnly.Hour, timeOnly.Minute);
                await Entity.UpdateAsync(appointment.Id, appointment);

                //  Update StartTime in CheckInLog
                var peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();
                List<VisitorLog> visitorLogs = peopleDb.VisitorLogs.Where(m => m.AppointmentId == AppointmentId).ToList();
                foreach (VisitorLog visitorLog in visitorLogs)
                {
                    visitorLog.StartTime = new TimeOnly(timeOnly.Hour, timeOnly.Minute);
                    await Entity.UpdateAsync(visitorLog.Id, visitorLog);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task CloseMeeting(CloseMeetingModel closeMeetingModel)
        {
            try
            {
                //  Update EndTime in Appointment
                Appointment appointment = await Entity.Query<Appointment>().FirstOrDefaultAsync(m => m.Id == closeMeetingModel.AppointmentId);
                appointment.Status = "Closed";
                appointment.EndTime = (closeMeetingModel.EndTime.HasValue) ? closeMeetingModel.EndTime : null;

                //appointment.EndTime = closeMeetingModel.EndTime.Add(TimeSpan.FromHours(5.5));
                await Entity.UpdateAsync(appointment.Id, appointment);

                //  Update EndTime in CheckInLog
                var peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();
                List<VisitorLog> visitorLogs = peopleDb.VisitorLogs.Where(m => m.AppointmentId == closeMeetingModel.AppointmentId).ToList();
                foreach (VisitorLog visitorLog in visitorLogs)
                {
                    visitorLog.EndTime = closeMeetingModel.EndTime;
                    await Entity.UpdateAsync(visitorLog.Id, visitorLog);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
