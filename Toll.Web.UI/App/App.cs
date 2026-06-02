using AppBuilder.Framework.MVC;
using Toll.Web.UI.Controllers;
using Toll.Web.UI.Data;

namespace Toll.Web.UI.App
{
    public class App : WebApp
    {

        public App()
        {
            this.AddAppInfo().SetApplicationName("Toll App");
            this.AddSqlDatabase<ControlPanelDb>();
            
                
            this.AddSqlDatabase<TollDbUIContext>();

            //this.AddIdentityDatabase<IdentityDb>()
            //   .AddDefaultUi()
            //   .LoginScreen()
            //   .SetLoginImageUrl("/Images/unifynow-right-image.png")
            //   .SetCompanyLogoUrl("/Images/unifynow-logo.png");
            //this.AddSingleSignon<IdentityDb>("UnifyApp");
            // this.DefaultMenu.AddMenu<GalleryController>("Gallery", "fa-solid fa-envelope");
            this.DefaultMenu.AddMenu<CustomerController>("Customer User");
            //this.DefaultMenu.AddMenu<BoothController>("Booths", "fa-solid fa-person-booth");
            //this.DefaultMenu.AddMenu<ShiftController>("Shifts", "fa-solid fa-hourglass-end");
            this.DefaultMenu.AddMenu<VehicleTypeController>("Vehicle Type", "fa-solid fa-bus");
            this.DefaultMenu.AddMenu<VehicleBillingController>("Vehicle Billing", "fa-solid fa-money-bill");
            this.DefaultMenu.AddMenu<DataController>("Daily Report", "fa-solid fa-database");
            this.DefaultMenu.AddMenu<MonthlyController>("Monthly Report", "fa-solid fa-database");
            this.DefaultMenu.AddMenu<ConfigurationController>("Configurations", "fa-solid fa-screwdriver-wrench");
        }
    }
}
