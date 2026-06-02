using AppBuilder.Framework.MVC.Controllers;
using AppBuilder.Model;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Toll.Web.UI.Entity;

namespace Toll.Web.UI.Controllers
{
    public class VehicleBillingController : PageController<VehicleBillingPage>
    {

    }

    public class VehicleBillingPage : PageModel
    {
        public VehicleBillingPage()
        {
            this.Title = "Vehicle Billing";
            this.GridView.AllowAdd(false);
            this.FormView.AllowAdd(false);
            this.FormView.AllowEdit(false);
            this.FormView.AllowDelete(false);
        }
        public GridView<VehicleBilling> GridView { get; set; } = new();
        public FormView<VehicleBilling> FormView { get; set; } = new();
    }
} 
        
      