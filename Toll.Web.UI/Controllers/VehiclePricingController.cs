using AppBuilder.Framework.MVC.Controllers;
using AppBuilder.Model;
using Toll.Web.UI.Entity;

namespace Toll.Web.UI.Controllers
{

    public class VehiclePricingController : PageController<VehiclePricingPage>
    {

    }

    public class VehiclePricingPage : PageModel
    {
        public VehiclePricingPage()
        {
            this.Title = "Vehicle Pricing";
        }
        public GridView<VehiclePricing> GridView { get; set; } = new();
        public FormView<VehiclePricing> FormView { get; set; } = new();
    }
} 

      