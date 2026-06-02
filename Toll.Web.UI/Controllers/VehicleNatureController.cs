using AppBuilder.Framework.MVC.Controllers;
using AppBuilder.Model;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Toll.Web.UI.Entity;

namespace Toll.Web.UI.Controllers
{

    public class VehicleNatureController : PageController<VehicleNaturePage>
    {

    }

    public class VehicleNaturePage : PageModel
    {
        public VehicleNaturePage()
        {
            this.Title = "Vehicle Nature";
        }
        public GridView<VehicleNature> GridView { get; set; } = new();
        public FormView<VehicleNature> FormView { get; set; } = new();
    }
} 
