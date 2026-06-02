using AppBuilder.Framework.Core;
using AppBuilder.Framework.MVC.Controllers;
using AppBuilder.Model;
using Microsoft.AspNetCore.Mvc;
using Toll.Web.UI.Entity;
using Toll.Web.UI.Services;

namespace Toll.Web.UI.Controllers
{
    public class CustomerController : PageController<CustomerPage>
    {

    }

    public class CustomerPage : PageModel
    {
        public CustomerPage()
        {
            this.Title = "Customer Users";
            this.OnPageLoad += CustomerPage_OnPageLoad;

          

        }

        private async Task CustomerPage_OnPageLoad()
        {
            this.GridView.OnLoadQuery += GridView_OnLoadQuery;
            var customerUser = FormView.GetPart<FormView<CustomerUser>>();
            var control = customerUser.GetFieldControl(m => m.UserRoles);
            var roles = GetRoleNames().Result;
            control.SetDataSource(roles);
            customerUser.OnAfterInsert += CustomerUser_OnAfterInsert;
        }

        private IQueryable<Customer> GridView_OnLoadQuery()
        {
            throw new NotImplementedException();
        }

        //private async Task Page_OnPageLoad()
        //{

        //    var customerUser = FormView.GetPart<FormView<CustomerUser>>();
        //    customerUser.OnAfterInsert += CustomerUser_OnAfterInsert;
        //}

        private async Task CustomerUser_OnAfterInsert(CustomerUser entity, IFormFileCollection files, string parentId)
        {
            try
            {
                var userService = CoreServices.GetService<UserService>();
                CustomerUser customeUser = entity as CustomerUser;


                CreateUserModel user = new()
                {
                    UserId = customeUser.Id.ToString(),
                    TenantId = "1a6612ab-e6fd-45a9-bd3c-ee90ad3c40b5",
                    CustomerId = "343d95d5-ad5d-4896-b917-fa81eea20677",
                    CustomerName = customeUser.Name,
                    UserType = "Customer User",
                    UserName = customeUser.Name,
                    Email = customeUser.Email,
                    Password = "User@123",
                    PhoneNumber = customeUser.PhoneNo,
                    Roles = customeUser.UserRoles
                };

                List<string> apps = [];
                //foreach (string str in customeUser.UserRoles)
                //{
                //    if (str != null)
                //    {
                //        apps.Add(str[..str.IndexOf('_')]);
                //    }
                //}
                apps.Add(customeUser.UserRoles.First());
                user.Applications = apps.Distinct();

                var result = await userService.CreateUserAsync(user);

            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<List<string>> GetRoleNames()
        {
            var userService = CoreServices.GetService<UserService>();
            return await userService.GetRolesNamesAsync();
        }


        public GridView<Customer> GridView { get; set; } = new();
        public FormView<Customer> FormView { get; set; } = new();
    }
}
