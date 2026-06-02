using AppBuilder.Framework.MVC.Controllers;
using AppBuilder.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Toll.Web.UI.Entity;

namespace Toll.Web.UI.Controllers
{

    public class ShiftController : PageController<ShiftsPage>
    {

    }

    public class ShiftsPage : PageModel
    {
        public ShiftsPage()
        {
            this.Title = "Shifts";
        }
        public GridView<Shift> GridView { get; set; } = new();
        public FormView<Shift> FormView { get; set; } = new();
    }
} 

