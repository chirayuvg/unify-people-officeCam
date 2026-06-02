using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using Microsoft.AspNetCore.Mvc;
using Unify.STR.Attendance.Data.SQL;

namespace Unify.STR.Attendance.Controllers.Attendance
{
    public class TaskController : PageController
    {
        public override void Init()
        {
            this.Page.Title = "Task List";

            var formView = this.AddFormView<Tasks>().SetLayout(FormLayout.Tab);
        }
    }
}
