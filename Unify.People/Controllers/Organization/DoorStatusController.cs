using AppStudio.Framework.Core;
using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using Unify.Data;
using Unify.Data.SQL;

namespace Unify.People.Controllers.Organization
{
    public class DoorStatusController : PageController
    {
        private CardView<DoorStatus> cardView;

        public override void Init()
        {
            this.Page.SetTitle("Door Status");

            var peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();
            DoorStatus doorStatus = peopleDb.DoorStatuses.FirstOrDefault();

            cardView = this.AddCardView<DoorStatus>();
            cardView.Data.Status = doorStatus.Status;
            cardView.OnSubmit += CardView_OnSubmit;

            UiAction.AddJsCode(@"
                setTimeout(function () {
                    window.location.reload();
                }, 10000);
            ");
        }

        public override Task ProcessAsync()
        {
            return base.ProcessAsync();
        }

        private async Task CardView_OnSubmit(DoorStatus model, IFormFileCollection files)
        {
            var peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();
            DoorStatus doorStatus = peopleDb.DoorStatuses.FirstOrDefault();
            doorStatus.Status = model.Status;
            await Entity.UpdateAsync(doorStatus.Id, doorStatus);
        }
    }
}
