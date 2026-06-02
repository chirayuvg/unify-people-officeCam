using AppBuilder.Framework.Core;
using AppBuilder.Framework.MVC.Controllers;
using AppBuilder.Model;
using Toll.Web.UI.Entity;

namespace Toll.Web.UI.Controllers
{
    public class VehicleTypeController : PageController<VehicleTypePage>
    {

    }

    public class VehicleTypePage : PageModel
    {
        public VehicleTypePage()
        {
            this.Title = "Vehicle Type";
        }
        public GridView<VehicleType> GridView { get; set; } = new();
        public FormView<VehicleType> FormView { get; set; } = new();
    }
}