using AppStudio.Enterprise.Data;
using AppStudio.Framework.Core;
using AppStudio.Framework.Identity.Data;
using AppStudio.Framework.Identity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Security.Claims;

namespace Unify.STR.Attendance.Services
{
    public class IdentityServices
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        public IdentityServices(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        public async Task<Result> CreateIdentityUserAsync(TenantUser tenantUser)
        {
            Result result = new() { IsSuccess = true };

            try
            {
                var rep = CoreServices.GetRepository<Tenant>();
                var Tenant = await rep.FindByIdAsync(tenantUser.TenantId);

                var model = new CreateIdentityUserModel()
                {
                    Id = tenantUser.Id,
                    UserName = tenantUser.Name,
                    Email = tenantUser.Email,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    Password = tenantUser.Password,
                   // Roles = tenantUser.Roles.Split(',').ToList(),
                    TenantId = tenantUser.TenantId,
                };


                model.Claims.Add(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", tenantUser.Id.ToString()));
                model.Claims.Add(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", tenantUser.Name));
                model.Claims.Add(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", tenantUser.Email));
                model.Claims.Add(new Claim("UserId", tenantUser.Id.ToString()));
                model.Claims.Add(new Claim("UserName", tenantUser.Name));
                model.Claims.Add(new Claim("TenantName", Tenant.Name));
                model.Claims.Add(new Claim("TenantType", Tenant.TenantType.ToString()));

                if (tenantUser.TenantId != null)
                {
                    model.Claims.Add(new Claim("TenantId", tenantUser.TenantId.ToString()));
                }

                ApplicationUser user = new()
                {
                    Id = model.Id.ToString(),
                    UserName = model.UserName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    EmailConfirmed = model.EmailConfirmed,
                    PhoneNumberConfirmed = model.PhoneNumberConfirmed,
                    NormalizedUserName = model.UserName.ToUpper(),
                    NormalizedEmail = model.Email.ToUpper(),
                    SecurityStamp = Guid.NewGuid().ToString(),
                };
                var idResult = await _userManager.CreateAsync(user, model.Password);
                if (idResult.Succeeded)
                {
                    if (model.Roles.Count > 0)
                    {
                        try
                        {
                            var roleresult = await _userManager.AddToRolesAsync(user, model.Roles);
                        }
                        catch (Exception ex)
                        {
                        }
                    }

                    if (model.Claims.Count > 0)
                    {
                        var roleresult = _userManager.AddClaimsAsync(user, model.Claims);
                    }
                    if (model.RequireConfirmedAccount)
                    {
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        var callbackUrl = CoreServices.GetUrlFromAction("ConfirmEmail", "Account", new { userId = user.Id, code = code });
                        await CoreServices.Notification.SendEmailAsync(user.Email, "Confirm your account",
                            "Please confirm your account by clicking this link: <a href=\"" + callbackUrl + "\">link</a>");

                        var phoneToken = _userManager.GenerateChangePhoneNumberTokenAsync(user, user.PhoneNumber);
                    }
                    else if (model.SendResetPasswordLinkAsync)
                    {
                        await SendResetPasswordLinkAsync(user.Email);
                    }
                }
                else
                {
                    var errors = idResult.Errors.Select(m => new Error("", m.Description)).ToList();
                    result.IsSuccess = false;
                    result.Errors = errors;
                }
                return result;

            }
            catch (Exception ex)
            {
                result.Errors.Add(new Error("", ex.Message));
            }
            return result;
        }
        public async Task<bool> SendResetPasswordLinkAsync(string email)
        {
            try
            {
                string url = "/Identity/Acccount/ResetPassword";
                await _emailSender.SendEmailAsync(email, "ResetPassword", $"Please reset your password by <a href='{url}'>clicking here</a>.");
                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
