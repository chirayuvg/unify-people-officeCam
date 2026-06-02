using AppStudio.Framework.Core;
using AppStudio.Framework.Identity.Data;
using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Unify.People.Controllers.Password
{
    public class ChangePasswordController : PageController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private CardView<ChangePasswordModel> cardView;

        public ChangePasswordController()
        {
            _userManager = CoreServices.GetService<UserManager<ApplicationUser>>();
            _signInManager = CoreServices.GetService<SignInManager<ApplicationUser>>();
        }

        public override void Init()
        {
            this.Page.SetTitle("Change Password");

            cardView = this.AddCardView<ChangePasswordModel>();
            cardView.SetColumns(1);
            cardView.SubmitOnDataChange = false;
            cardView.OnSubmit += CardView_OnSubmit;
        }

        private async Task CardView_OnSubmit(ChangePasswordModel model, IFormFileCollection files)
        {
            var user = await _userManager.GetUserAsync(User);

            var resultCheckPassword = await _signInManager.CheckPasswordSignInAsync(user, model.OldPassword, false);

            if (resultCheckPassword.Succeeded)
            {
                IdentityResult resultChangePassword = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (resultChangePassword.Succeeded)
                {
                    await _signInManager.RefreshSignInAsync(user);

                    UiAction.AddJsCode("alert('Password updated successfully.');");

                    cardView.Data.OldPassword = "";
                    cardView.Data.NewPassword = "";
                    cardView.Data.ReEnterNewPassword = "";
                }
                else
                {
                    string errorMessage = "";
                    foreach (var error in resultChangePassword.Errors) errorMessage += "#" + error.Description + "\\n";
                    errorMessage = errorMessage.Replace("'", "");
                    UiAction.AddJsCode($"alert('{errorMessage}');");

                    cardView.Data.NewPassword = "";
                    cardView.Data.ReEnterNewPassword = "";
                }
            }
            else
            {
                UiAction.AddJsCode("alert('Old Password is not correct. Please try again.');");
            }
        }
    }

    public class ChangePasswordModel
    {
        [Required(ErrorMessage = "Old Password is required")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "New Password is required")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Please re-enter the new password")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ReEnterNewPassword { get; set; }
    }
}
