//using AppBuilder.Framework.Identity.Models;
//using AppBuilder.Framework.MVC;
using AppStudio.Enterpise.Module.Data;
using AppStudio.Framework.Identity.Models;
using AppStudio.Framework.MVC;
using AppStudio.Model;
using Unify.CCC.Data;
using UnifyNow.Forest.Controllers;
using UnifyNow.Forest.Controllers.APIController;
using UnifyNow.Forest.Data.Elasticsearch;
using UnifyNow.Forest.Data.SQL;

namespace UnifyNow.Forest.App
{
    public class App : WebApp
    {

        public App()
        {

            this.AddApplication("Wildlife AI", "Wildlife AI")
               .SetApplicationLogoUrl("/Images/unifynow-logo.png");


            this.AddMessaging().UseRabbitMQ();
            //this.AddEnterpriseServices<ApplicationDb>(DatabaseType.SqlServer);
            
            this.AddDatabase<ForestDb>(DatabaseType.SqlServer);
            this.AddDatabase<CCCDb>(DatabaseType.SqlServer);
            this.AddElasticSearchDatabase<ElasticDb>();
            //this.AddIdentityDatabase<IdentityDb>()

            // this.DefaultMenu.AddMenu<GalleryController>("Gallery", "fa-solid fa-envelope");
            this.AddMenu<MemeberController>("Members", "fa-solid fa-person");
            this.AddMenu<TeamController>("Teams", "fa-solid fa-users-gear");

            var dept = this.AddSubMenu("Departments", "fa-solid fa-building");
            dept.AddMenu<DesignationController>("Designation", "fa-solid fa-bars-progress");
            dept.AddMenu<DepartmentController>("Department", "fa-solid fa-building");

            var camera = this.AddSubMenu("Camera", "fa-solid fa-camera-retro");
            camera.AddMenu<AreasController>("Area", "fa-solid fa-location-dot");
               camera.AddMenu<CamerasController>("Camera", "fa-solid fa-camera-retro");
            //.AddMenu<PresetController>("Preset", "fa-solid fa-gears");

            var alerts = this.AddSubMenu("Alerts", "fa-regular fa-bell");
            alerts.AddMenu<AnimalsController>("Animals", "fa-solid fa-paw");
               alerts.AddMenu<AlertRulesController>("AI Intelligence", "fa-solid fa-snowflake");

            //this.AddMenu<WhatsAppController>("Notifications", "fa-regular fa-bell");

        }
    }
}
