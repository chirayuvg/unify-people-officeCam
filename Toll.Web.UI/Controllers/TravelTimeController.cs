using AppBuilder.Framework.MVC.Controllers;
using AppBuilder.Model;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Toll.Web.UI.Entity;

namespace Toll.Web.UI.Controllers
{
    public class TravelTimeController : PageController<TravelTimePage>
    {

    }

    public class TravelTimePage : PageModel
    {
        public TravelTimePage()
        {
            this.Title = "Travel Time";
        }
        public GridView<TravelTime> GridView { get; set; } = new();
        public FormView<TravelTime> FormView { get; set; } = new();
    }
} 
      