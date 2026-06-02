using AppBuilder.Framework.MVC.Controllers;
using AppBuilder.Model;
using Toll.Web.UI.Entity;

namespace Toll.Web.UI.Controllers
{

    public class VehicleController : PageController<VehiclePage>
    {

    }

    public class VehiclePage : PageModel
    {
        public VehiclePage()
        {
            this.Title = "Exemptions";
        }
        public GridView<Vehicle> GridView { get; set; } = new();
        public FormView<Vehicle> FormView { get; set; } = new();
    }
} 

