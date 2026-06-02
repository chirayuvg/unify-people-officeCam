using AppBuilder.Framework.MVC.Controllers;
using AppBuilder.Model;
using Microsoft.AspNetCore.Mvc;
using Toll.Web.UI.Entity;

namespace Toll.Web.UI.Controllers
{
    public class ConfigurationController : PageController<SettingPage>
    {

    }

    public class SettingPage : PageModel
    {
        public SettingPage()
        {
            this.Title = "Configurations";

        }
        public GridView<AppSetting> GridView { get; set; } = new();
        public FormView<AppSetting> FormView { get; set; } = new();
    }
}
