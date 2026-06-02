using AppBuilder.Framework.MVC.Controllers;
using AppBuilder.Model;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Toll.Web.UI.Entity;

namespace Toll.Web.UI.Controllers
{
    public class BoothController : PageController<BootsPage>
    {

    }

    public class BootsPage : PageModel
    {
        public BootsPage()
        {
            this.Title = "Booths";
        }
        public GridView<Booth> GridView { get; set; } = new();
        public FormView<Booth> FormView { get; set; } = new();
    }
}
