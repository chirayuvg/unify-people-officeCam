using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Toll.Web.UI.Services
{
    public class UserService
    {
        UserManager<IdentityUser> _userManager;
        RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        public UserService(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager,
             IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        public async Task<bool> CreateUserAsync(CreateUserModel model)
        {
            try
            {

                IdentityUser user = new IdentityUser
                {
                    Id = model.UserId.ToString(),
                    UserName = model.Email,
                    Email = model.Email,
                    NormalizedUserName = model.UserName.ToUpper(),
                    NormalizedEmail = model.Email.ToUpper(),
                    EmailConfirmed = true,
                    LockoutEnabled = false,
                    PhoneNumberConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString()
                };
                if ((await _userManager.CreateAsync(user, model.Password)).Succeeded)
                {
                    await _userManager.AddClaimAsync(user, new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", model.UserId));
                    await _userManager.AddClaimAsync(user, new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", model.UserName));
                    await _userManager.AddClaimAsync(user, new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", model.Email));
                    await _userManager.AddClaimAsync(user, new Claim("UserId", model.UserId));
                    if (model.Roles != null)
                    {
                        foreach (string role in model.Roles)
                        {
                            string[] roles = role.Split(',');
                            foreach (string role2 in roles)
                            {
                                await _userManager.AddToRoleAsync(user, role2);
                            }
                        }
                    }
                    if (model.Applications != null)
                    {
                        foreach (string app in model.Applications)
                        {
                            await _userManager.AddClaimAsync(user, new Claim("Application", app));
                        }
                    }

                    await _userManager.AddClaimAsync(user, new Claim("UserName", model.UserName));
                    if (model.TenantId != null)
                    {
                        await _userManager.AddClaimAsync(user, new Claim("TenantId", model.TenantId));
                    }

                    if (model.CustomerId != null)
                    {
                        await _userManager.AddClaimAsync(user, new Claim("CustomerId", model.CustomerId));
                    }

                    if (model.CustomerName != null)
                    {
                        await _userManager.AddClaimAsync(user, new Claim("CustomerName", model.CustomerName));
                    }

                    if (model.UserType != null)
                    {
                        await _userManager.AddClaimAsync(user, new Claim("UserType", model.UserType));
                    }
                }

                //var saasdb = CoreServices.GetService<ControlPanelDb>();
                //CustomerUser customerUser = new CustomerUser();
                //customerUser.Id = Guid.Parse(user.Id);
                //customerUser.CustomerId = Guid.Parse(model.CustomerId);
                //customerUser.Name = model.UserName;
                //customerUser.Email = model.Email;
                //customerUser.UserRoles = model.Roles.ToList();
                //customerUser.IsActive = true;
                //await saasdb.CustomerUsers.AddAsync(customerUser);
                //await saasdb.SaveChangesAsync();


            }
            catch (Exception ex)
            {

                var error = ex.Message;
            }
            return false;
        }
        public async Task<bool> CreateCPUser(CreateUserModel model)
        {
            try
            {
                IdentityUser user = new IdentityUser
                {
                    Id = model.UserId.ToString(),
                    UserName = model.Email,
                    Email = model.Email,
                    NormalizedUserName = model.UserName.ToUpper(),
                    NormalizedEmail = model.Email.ToUpper(),
                    EmailConfirmed = true,
                    LockoutEnabled = false,
                    PhoneNumberConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString()
                };
                if ((await _userManager.CreateAsync(user, model.Password)).Succeeded)
                {
                    await _userManager.AddClaimAsync(user, new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", model.UserId));
                    await _userManager.AddClaimAsync(user, new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", model.UserName));
                    await _userManager.AddClaimAsync(user, new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", model.Email));
                    await _userManager.AddClaimAsync(user, new Claim("UserId", model.UserId));
                    foreach (string role in model.Roles)
                    {
                        string[] roles = role.Split(',');
                        foreach (string role2 in roles)
                        {
                            await _userManager.AddToRoleAsync(user, role2);
                        }
                    }

                    foreach (string app in model.Applications)
                    {
                        await _userManager.AddClaimAsync(user, new Claim("Application", app));
                    }

                    await _userManager.AddClaimAsync(user, new Claim("UserName", model.UserName));
                    if (model.TenantId != null)
                    {
                        await _userManager.AddClaimAsync(user, new Claim("TenantId", model.TenantId));
                    }

                    if (model.UserType != null)
                    {
                        await _userManager.AddClaimAsync(user, new Claim("UserType", model.UserType));
                    }
                }

                //var saasdb = CoreServices.GetService<ControlPanelDb>();
                //CPUser cpUser = new CPUser();
                //cpUser.Id = Guid.Parse(user.Id);
                //cpUser.Name = model.UserName;
                //cpUser.Email = model.Email;
                //cpUser.Roles = model.Roles.ToArray();

                //await saasdb.CPUsers.AddAsync(cpUser);
                //await saasdb.SaveChangesAsync();


            }
            catch (Exception ex)
            {

                var error = ex.Message;
            }
            return false;
        }


        //public async Task<bool> ResetPasswordAsync(string email)
        //{
        //    try
        //    {
        //        string url = _configuration.GetValue<string>("ForgotPasswordUrl");
        //        await _emailSender.SendEmailAsync(email, "ResetPassword", $"Please reset your password by <a href='{url}'>clicking here</a>.");
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}

        public async Task<List<string>> GetRolesNamesAsync()
        {
            return await _roleManager.Roles.Select(m => m.Name).ToListAsync();
        }

        public async Task<IdentityRole?> CreateRoleAsync(string roleName)
        {
            var role = new IdentityRole() { Name = roleName, NormalizedName = roleName.ToUpper() };

            var result = await _roleManager.CreateAsync(role);

            if (result.Succeeded)
            {
                return role;
            }

            return null;
        }
    }

    public class CreateUserModel
    {
        public string UserId { get; set; }
        public string TenantId { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string UserType { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public IEnumerable<string> Roles { get; set; } = new List<string>();
        public IEnumerable<string> Applications { get; set; } = new List<string>();

        public Dictionary<string, string> AdditionalClaims { get; set; } = new Dictionary<string, string>();
    }
}
