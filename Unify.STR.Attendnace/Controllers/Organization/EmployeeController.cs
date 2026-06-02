using AppStudio.Enterprise.Data;
using AppStudio.Enterprise.Services;
using AppStudio.Framework.Core;
using AppStudio.Framework.Identity.Data;
using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using DevExtreme.AspNet.Mvc;
using DTC.Unify.HttpClientService;
using Elastic.Clients.Elasticsearch.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Razorpay.Api;
using System.ComponentModel.DataAnnotations;
using Unify.STR.Attendance.Data.SQL;
using Unify.STR.Attendance.Services;
using Range = Unify.STR.Attendance.Data.SQL.Range;


namespace Unify.STR.Attendance.Controllers.Organization
{
    public class EmployeeController : PageController
    {
        private readonly EmployeeService _employeeService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IdentityServices _identityServices;
        private readonly ApplicationDb _applicationDb;
        public EmployeeController(UserManager<ApplicationUser> userManager, IdentityServices identityServices, IDbContextFactory<ApplicationDb> applicationDb)
        {
            _userManager = userManager;
            _identityServices = identityServices;
            _applicationDb = applicationDb.CreateDbContext();
        }

        public override void Init()
        {
            this.Page.Title = "Employee List";

            var formView = this.AddFormView<Employee>().SetLayout(FormLayout.Tab);
            
            formView.OnBeforeInsert += FormView_OnBeforeInsert;
            formView.OnBeforeDelete += FormView_OnBeforeDelete;
            //formView.OnAfterInsert += FormView_OnAfterInsert;
            //formView.OnBeforeInsert += FormView_OnBeforeInsert;
            var EmployeeImageFormView = formView.FindControl<FormView<EmployeeImage>>() as FormView<EmployeeImage>;

           
            formView.GetFieldControl(m => m.CircleId).SetCascade(nameof(Employee.DivisionId));
            formView.GetFieldControl(m => m.DivisionId).SetCascade(nameof(Employee.RangeId));
            formView.GetFieldControl(m => m.RangeId).SetCascade(nameof(Employee.SectionId));
            formView.GetFieldControl(m => m.SectionId).SetCascade(nameof(Employee.BeatId));
            //EmployeeService emp = CoreServices.GetService<EmployeeService>();
            //var result = emp.GetRange(Division.Id)
            //Range.SetDataSource<Range>(m => m.TenantId, m => m.TenantId, m => m.IsActive == true)
            //    .SetDisplayMember(m => m.Name)
            //    .SetValueMember(m => m.Id);
            EmployeeImageFormView.OnBeforeInsert += EmployeeImageFormView_OnBeforeInsert;
            // EmployeeImageFormView.OnBeforeDelete += EmployeeImageFormView_OnBeforeDelete;
           // EmployeeImageFormView.OnAfterInsert += EmployeeImageFormView_OnAfterInsert;

            var gridView = this.AddGridView<Employee>()
                .SetFormView(formView)
                .ConfigueUI(
                    grid => grid
                        .ColumnWidth(Mode.Auto)
                        .FilterRow(filterRow => filterRow
                            .ApplyFilter(GridApplyFilterMode.Auto)
                            .Visible(false)
                        )
                        .HeaderFilter(headerFilter => headerFilter.Visible(false))
                        .RowAlternationEnabled(true)
                        .SearchPanel(s => s.Visible(true))
                        .ShowBorders(true)
                        .ShowColumnLines(false)
                        .ShowRowLines(false)
                        .WordWrapEnabled(true).Width("100%")
                );
        }

        private async Task<Result> FormView_OnBeforeDelete(object id)
        {
            Result result = new Result();
            ApplicationUser entity = await _userManager.FindByIdAsync(id.ToString());
            if (entity != null)
            {
                var claims = await _userManager.GetClaimsAsync(entity);
                foreach (var claim in claims)
                {
                    await _userManager.RemoveClaimAsync(entity, claim);
                }
                var roles = await _userManager.GetRolesAsync(entity);
                if(roles != null && roles.Count > 0)
                {
                    foreach (var role in roles)
                    {
                        await _userManager.RemoveFromRoleAsync(entity, role);
                    }
                }

                var data =  await _userManager.DeleteAsync(entity);
                if(data.Succeeded == false)
                {
                    result.IsSuccess = false;
                    foreach (var error in data.Errors)
                    {
                        result.AddError(error.Code, error.Description);
                    }
                    return result;
                }
            }
            return result;
        }

        private  async Task<Result> FormView_OnBeforeInsert(Employee entity, IFormFileCollection files, string parentId)
        {
            entity.TenantId = CoreServices.CurrentUser.GetTenantId();
            Result result = new Result();
            var email = (entity.MobileNo != null ? entity.MobileNo.ToString() : "") + "@str.com";
            var user = await _userManager.FindByEmailAsync(email);
            if(user != null)
            {
                result.IsSuccess = false;
                result.AddError("MobileNo", "MobileNo is already in use.");
                return result;
            }
            Employee employee = entity as Employee;
            string password = string.Empty;

            if (employee.MobileNo != null)
            {
                string mobileStr = employee.MobileNo.ToString();
                password = mobileStr.Length >= 4 ? mobileStr.Substring(mobileStr.Length - 4) : mobileStr;
            }
            TenantUser tenantUser = new()
            {

                Id = employee.Id,
                Name = employee.Name,
                Email = (employee.MobileNo != null ? employee.MobileNo.ToString() : "") + "@str.com",
                Password = password,
                IsActive = true,
                TenantId = employee.TenantId,
                Roles = ["User"]
            };
            var service = CoreServices.GetService<TenantUserService>();
            var res = await service.CreateIdentityUserAsync(tenantUser);
            if (res.IsSuccess == false)
            {
                result.IsSuccess = false;
                result.AddError("MobileNo", "MobileNo is already in use.");
                return result;
            }
            return result;
        }

        private async Task<Result> EmployeeImageFormView_OnBeforeInsert(EmployeeImage entity, IFormFileCollection files, string parentId)
        {
            try
            {
                FaceService faceService = CoreServices.GetService<FaceService>();

                var embeddings = await faceService.GetEmbeddings(files[0]);
                Vector vector = new Vector(embeddings);

                entity.Embedding = vector;

            }
            catch (Exception ex)
            {
                var error = ex.Message;
            }
            return new Result();
        }

        //private async Task<Result> FormView_OnBeforeInsert(Employee entity, IFormFileCollection files, string parentId)
        //{
        //    Employee employee = entity as Employee;
        //    TenantUser tenantUser = new()
        //    {
        //        Id = employee.Id,
        //        Name = employee.Name,
        //        Email = employee.Email,
        //        Password = "Str@123",
        //        IsActive = true,
        //        TenantId = employee.TenantId,
        //    };
        //    var result = await Entity.InsertAsync(tenantUser);
        //    if (result.IsSuccess == true)
        //    {
        //        return new Result();
        //    }
        //    else
        //    {
        //        return new Result { IsSuccess = false };
        //    }
        //}

        private async Task FormView_OnAfterInsert(Employee entity, IFormFileCollection files, string parentId)
        {
            Employee employee = entity as Employee;
            string password = string.Empty;

            if (employee.MobileNo != null)
            {
                string mobileStr = employee.MobileNo.ToString();
                password = mobileStr.Length >= 4 ? mobileStr.Substring(mobileStr.Length - 4) : mobileStr;
            }
            TenantUser tenantUser = new()
            {
                Id = employee.Id,
                Name = employee.Name,
                Email = (employee.MobileNo != null ? employee.MobileNo.ToString() : "") + "@str.com",
                Password = password,
                IsActive = true,
                TenantId = employee.TenantId,
            };
            var service = CoreServices.GetService<TenantUserService>();
           var res =   await service.CreateIdentityUserAsync(tenantUser);
        }
        //private async Task FormView_OnAfterInsert(Employee entity, IFormFileCollection files, string parentId)
        //{
        //    Employee employee = entity as Employee;
        //    TenantUser tenantUser = new()
        //    {
        //        Id = employee.Id,
        //        Name = employee.Name,
        //        Email = employee.Email,
        //        Password = "Str@123",
        //        IsActive = true,
        //        TenantId = employee.TenantId,
        //    };
        //    await Entity.InsertAsync(tenantUser);
        //}

        private async Task EmployeeImageFormView_OnAfterInsert(EmployeeImage entity, IFormFileCollection files, string parentId)
        {
            try
            {
                FaceService faceService = CoreServices.GetService<FaceService>();

                var embeddings = await faceService.GetEmbeddings(files[0]);
                Vector vector = new Vector(embeddings);

                entity.Embedding = vector;

            }
            catch (Exception ex)
            {
                var error = ex.Message;
            }

        }
        //private async Task<Result> EmployeeImageFormView_OnBeforeInsert(EmployeeImage entity, IFormFileCollection files, string parentId)
        //{

        //    try
        //    {
        //          FaceService faceService = CoreServices.GetService<FaceService>();

        //        var embeddings = await faceService.GetEmbeddings(files[0]);
        //        Vector vector = new Vector(embeddings);

        //        entity.Embedding = vector;

        //    }
        //    catch (Exception ex)
        //    {
        //        var error = ex.Message;
        //    }

        //    return new Result();
        //}
    }

    public class EmployeeImageRequest
    {
        public string? ImageId { get; set; }

        [Required]
        public IFormFile Image { get; set; }

        public string? EmployeeId { get; set; }

        public string? TenantId { get; set; }
    }
}
