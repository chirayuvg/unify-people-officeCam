using AppBuilder.Framework.MVC.Controllers;
using AppBuilder.Model;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Toll.Web.UI.Entity;

namespace Toll.Web.UI.Controllers
{
    public class CameraController : PageController<CamerasPage>
    {

    }

    public class CamerasPage : PageModel
    {
        public CamerasPage()
        {
            this.Title = "Cameras";
        }
        public GridView<Camera> GridView { get; set; } = new();
        public FormView<Camera> FormView { get; set; } = new();
    }
} 