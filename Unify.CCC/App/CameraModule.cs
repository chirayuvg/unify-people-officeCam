
using Unify.CCC.Controllers;
using Unify.CCC.Data;

using AppStudio.Framework.MVC;
using AppStudio.Model;
using Unify.CCC.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Unify.CCC
{
    public class CameraModule : WebModule
    {


        public override void Init(WebApp app)
        {

            app.AddDatabase<CCCDb>( DatabaseType.SqlServer);

            var cam = app.AddSubMenu("Camera", "fa-solid fa-camera");

            cam.AddMenu<CameraController>("Camera", "fa-solid fa-camera");
            cam.AddMenu<SegmentController>("Segment", "fa-solid fa-layer-group");
            cam.AddMenu<IntelligenceController>("Intelligence", "fa-solid fa-brain");
            cam.AddMenu<MonitorController>("Monitor", "fa-brands fa-watchman-monitoring");
            cam.AddMenu<ChannelController>("Channel", "fa-solid fa-walkie-talkie");
            cam.AddMenu<NotificationController>("Notification", "fa-solid fa-envelope");
            cam.AddMenu<TicketController>("Ticket", "fa-solid fa-ticket");

          


        }

        public override void ConfigureServices(IHostApplicationBuilder builder)
        {
            builder.Services.AddScoped<CameraServices>();
        }

    }
}
