using AppStudio.Framework.MVC.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace UnifyNow.Forest.Controllers
{
    public class GalleryPopupController : Controller
    {
        public GalleryPopupController()
        {
            //string xyxz = Request.Query["ImageFolderPath"];
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = new Image();
            model.ImageFolderPath = Request.Query["ImageFolderPath"];
            return PartialView(model);
        }
    }

    public class Image
    {
        public string ImageFolderPath { get; set; }
    }
}
