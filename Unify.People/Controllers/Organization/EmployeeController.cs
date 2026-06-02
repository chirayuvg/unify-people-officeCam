using AppStudio.Enterprise.Data;
using AppStudio.Framework.Core;
using AppStudio.Framework.Identity.Services;
using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using DevExtreme.AspNet.Mvc;
using DTC.Unify.HttpClientService;
using Pgvector;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Unify.Data;
using Unify.Data.SQL;

namespace Unify.People.Controllers.Organization
{
    public class EmployeeController : PageController
    {
        private readonly IdentityUserService _identityUserService;

        private FormView<Employee> formView;
        private GridView<Employee> gridView;

        public EmployeeController(IdentityUserService identityUserService)
        {
            _identityUserService = identityUserService;
        }

        public override void Init()
        {
            this.Page.SetTitle("Employee List");

            formView = this.AddFormView<Employee>().SetLayout(FormLayout.Tab);
            formView.OnBeforeInsert += FormView_OnBeforeInsert;
            formView.OnBeforeUpdate += FormView_OnBeforeUpdate;
            formView.OnBeforeDelete += FormView_OnBeforeDelete;
            formView.OnAfterInsert += FormView_OnAfterInsert;
            formView.OnAfterUpdate += FormView_OnAfterUpdate;

            var EmployeeImageFormView = formView.FindControl<FormView<EmployeeImage>>() as FormView<EmployeeImage>;
            EmployeeImageFormView.OnBeforeInsert += EmployeeImageFormView_OnBeforeInsert;
            EmployeeImageFormView.OnBeforeUpdate += EmployeeImageFormView_OnBeforeUpdate;

            gridView = this.AddGridView<Employee>()
                .SetFormView(formView)
                .ConfigueUI(
                    grid => grid
                        .ColumnAutoWidth(false)
                        .FilterRow(f => f.Visible(false))
                        .HeaderFilter(headerFilter => headerFilter.Visible(false))
                        .RowAlternationEnabled(true)
                        .Scrolling(scrolling => scrolling.ColumnRenderingMode(GridColumnRenderingMode.Virtual))
                        .SearchPanel(s => s.Visible(true).HighlightCaseSensitive(false))
                        .ShowBorders(true)
                        .ShowColumnHeaders(true)
                        .ShowColumnLines(false)
                        .ShowRowLines(false)
                        .WordWrapEnabled(true).Width("100%")
                        .Paging(paging => paging.Enabled(true).PageSize(10))
                        .Pager(pager =>
                        {
                            pager
                                .Visible(true)
                                .DisplayMode(GridPagerDisplayMode.Full)
                                .ShowPageSizeSelector(false)
                                .AllowedPageSizes(new JS("[5]"))
                                .ShowInfo(true)
                                .ShowNavigationButtons(true);
                        })
                );
            gridView.Css.SetHeight("calc(100vh - 200px)");
            gridView.Fields(field =>
            {
                field.ClearFields();

                field.AddField(f => f.Name).ColumnWidth = 200;
                field.AddField(f => f.Code).ColumnWidth = 100;
                field.AddField(f => f.Gender).ColumnWidth = 100;
                field.AddField(f => f.DoB).ColumnWidth = 100;
                field.AddField(f => f.EmployeeTypeId).ColumnWidth = 100;
                field.AddField(f => f.DepartmentId).ColumnWidth = 100;
                field.AddField(f => f.DesignationId).ColumnWidth = 100;
                field.AddField(f => f.ShiftId).ColumnWidth = 100;
                field.AddField(f => f.MobileNo).ColumnWidth = 100;
                field.AddField(f => f.Email).ColumnWidth = 100;
            });
            gridView.SetDefaultSort("Name", false);
        }

        public override Task ProcessAsync()
        {
            formView.OnBeforeInsert += FormView_OnBeforeInsert;
            formView.OnBeforeUpdate += FormView_OnBeforeUpdate;
            formView.OnBeforeDelete += FormView_OnBeforeDelete;

            return base.ProcessAsync();
        }

        private async Task<Result> FormView_OnBeforeInsert(Employee entity, IFormFileCollection files, string parentId)
        {
            var result = new Result();

            try
            {
                //  Check Duplicate
                var peopleDb = CoreServices.GetService<PeopleDb>();

                //  Check Duplicate Employee Code
                var employeeCode = peopleDb.Employees
                    .Where(m => m.Code.ToLower() == entity.Code.ToLower())
                    .FirstOrDefault()?.Code ?? "";
                if (entity.Code.ToLower() == employeeCode.ToLower())
                {
                    UiAction.AddJsCode("alert('Employee Code " + entity.Code + " already exists. Cannot insert it again.');");
                    result.AddError("EmployeeId", "Cannot Insert: Employee Code " + entity.Code + " already exists");
                    result.IsSuccess = false;
                    return result;
                }

                //  Check Duplicate Email
                var employeeEmail = peopleDb.Employees
                    .Where(m => m.Email.ToLower() == entity.Email.ToLower())
                    .FirstOrDefault()?.Email ?? "";
                if (entity.Email.ToLower() == employeeEmail.ToLower())
                {
                    UiAction.AddJsCode("alert('Email " + entity.Email + " already exists. Cannot insert it again.');");
                    result.AddError("EmployeeId", "Cannot Insert: Employee Email " + entity.Email + " already exists");
                    result.IsSuccess = false;
                    return result;
                }
            }
            catch (Exception ex)
            {
                UiAction.AddJsCode(ex.Message.ToString());
                result.AddError(ex.Message.ToString());
                result.IsSuccess = false;
                return result;
            }

            result.IsSuccess = true;
            return result;
        }

        private async Task<Result> FormView_OnBeforeUpdate(Employee entity, IFormFileCollection files, string parentId)
        {
            var result = new Result();

            try
            {
                //  Check Duplicate
                var peopleDb = CoreServices.GetService<PeopleDb>();

                //  Check Duplicate Employee Code
                var employeeCode = peopleDb.Employees
                    .Where(m =>
                        m.Code.ToLower() == entity.Code.ToLower()
                        &&
                        m.Id != entity.Id
                    )
                    .FirstOrDefault()?.Code ?? "";

                if (entity.Code.ToLower() == employeeCode.ToLower())
                {
                    UiAction.AddJsCode("alert('Employee Code " + entity.Code + " already exists. Cannot insert it again.');");
                    result.AddError("EmployeeId", "Cannot Insert: Employee Code " + entity.Code + " already exists");
                    result.IsSuccess = false;
                    return result;
                }

                //  Check Duplicate Email
                var employeeEmail = peopleDb.Employees
                    .Where(m =>
                        m.Email.ToLower() == entity.Email.ToLower()
                        &&
                        m.Id != entity.Id
                    )
                    .FirstOrDefault()?.Email ?? "";
                if (entity.Email.ToLower() == employeeEmail.ToLower())
                {
                    UiAction.AddJsCode("alert('Email " + entity.Email + " already exists. Cannot insert it again.');");
                    result.AddError("EmployeeId", "Cannot Insert: Employee Email " + entity.Email + " already exists");
                    result.IsSuccess = false;
                    return result;
                }
            }
            catch (Exception ex)
            {
                UiAction.AddJsCode(ex.Message.ToString());
                result.AddError(ex.Message.ToString());
                result.IsSuccess = false;
                return result;
            }

            result.IsSuccess = true;
            return result;
        }

        private async Task<Result> FormView_OnBeforeDelete(object id)
        {
            var result = new Result(); // Create a new result

            try
            {
                var peopleDb = CoreServices.GetService<PeopleDb>();

                DateTime fromDate = DateTime.Today.AddDays(-7);
                DateTime toDate = DateTime.Today;

                var getImage = peopleDb.EmployeeImages
                    .Where(a =>
                        a.EmployeeId == Guid.Parse(id.ToString())
                    )
                    .ToList();
                if (getImage.Any())
                {
                    UiAction.AddJsCode("alert('Please delete all the images before deleting the employee');");
                    result.AddError("EmployeeId", "Cannot delete: Please delete all the images before deleting the employee");
                    result.IsSuccess = false;
                    return result;
                }

                var relatedLogs = peopleDb.AttendanceLogs
                    .Where(a =>
                        a.EmployeeId == Guid.Parse(id.ToString())
                    )
                    .ToList();
                if (relatedLogs.Any())
                {
                    UiAction.AddJsCode("alert('This record exists in attendance logs, hence cannot be deleted');");
                    result.AddError("EmployeeId", "Cannot Delete: Record exists for this employee in attendance logs");
                    result.IsSuccess = false;
                    return result;
                }
            }
            catch (Exception ex)
            {
                UiAction.AddJsCode(ex.Message.ToString());
                result.AddError(ex.Message.ToString());
                result.IsSuccess = false;
                return result;
            }

            result.IsSuccess = true;
            return result;
        }

        private async Task FormView_OnAfterInsert(Employee entity, IFormFileCollection files, string parentId)
        {
            var TenantId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetTenantId();

            //  Create TenantUser
            TenantUser tenantUser = new()
            {
                Id = Guid.NewGuid(),
                Name = entity.Name,
                Email = entity.Email,
                Password = "Dtc@123",
                IsActive = true,
                IsSuperAdmin = false,
                TenantId = TenantId,
                Roles = ["User"],
            };
            await Entity.InsertAsync(tenantUser);

            //  Create Claims
            var rep = CoreServices.GetRepository<Tenant>();
            var Tenant = await rep.FindByIdAsync(TenantId);

            var model = new CreateIdentityUserModel()
            {
                Id = tenantUser.Id,
                UserName = tenantUser.Name,
                Email = tenantUser.Email,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                Password = tenantUser.Password,
                TenantId = tenantUser.TenantId
            };

            model.Roles.Add("User");
            model.Claims.Add(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", tenantUser.Id.ToString()));
            model.Claims.Add(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", tenantUser.Name));
            model.Claims.Add(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", tenantUser.Email));
            model.Claims.Add(new Claim("UserId", tenantUser.Id.ToString()));
            model.Claims.Add(new Claim("UserName", tenantUser.Name));
            model.Claims.Add(new Claim("TenantName", Tenant.Name));
            model.Claims.Add(new Claim("TenantType", Tenant.TenantType.ToString()));
            model.Claims.Add(new Claim("TenantId", tenantUser.TenantId.ToString()));

            await _identityUserService.CreateIdentityUserAsync(model);
        }

        private async Task FormView_OnAfterUpdate(Employee entity, IFormFileCollection files, string parentId)
        {
            TenantUser tenantUser = (await Entity.GetAllAsync<TenantUser>())
                .Where(m => m.Email == entity.Email)
                .FirstOrDefault();

            if (tenantUser == null)
            {
                var TenantId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetTenantId();

                //  Create TenandUser
                tenantUser = new()
                {
                    Id = Guid.NewGuid(),
                    Name = entity.Name,
                    Email = entity.Email,
                    Password = "Dtc@123",
                    IsActive = true,
                    IsSuperAdmin = false,
                    TenantId = TenantId,
                    Roles = ["User"],
                };
                await Entity.InsertAsync(tenantUser);

                //  Create Claims
                var rep = CoreServices.GetRepository<Tenant>();
                var Tenant = await rep.FindByIdAsync(TenantId);

                var model = new CreateIdentityUserModel()
                {
                    Id = tenantUser.Id,
                    UserName = tenantUser.Name,
                    Email = tenantUser.Email,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    Password = tenantUser.Password,
                    TenantId = tenantUser.TenantId
                };

                model.Roles.Add("User");
                model.Claims.Add(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", tenantUser.Id.ToString()));
                model.Claims.Add(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", tenantUser.Name));
                model.Claims.Add(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", tenantUser.Email));
                model.Claims.Add(new Claim("UserId", tenantUser.Id.ToString()));
                model.Claims.Add(new Claim("UserName", tenantUser.Name));
                model.Claims.Add(new Claim("TenantName", Tenant.Name));
                model.Claims.Add(new Claim("TenantType", Tenant.TenantType.ToString()));
                model.Claims.Add(new Claim("TenantId", tenantUser.TenantId.ToString()));

                await _identityUserService.CreateIdentityUserAsync(model);
            }
            else
            {
                tenantUser.Email = entity.Email;
                await Entity.UpdateAsync(tenantUser.Id, tenantUser);
            }
        }

        private async Task<Result> EmployeeImageFormView_OnBeforeInsert(EmployeeImage entity, IFormFileCollection files, string parentId)
        {
            var result = new Result();

            try
            {
                FaceService faceService = CoreServices.GetService<FaceService>();
                var embeddings = await faceService.GetEmbeddings(files[0]);

                if (embeddings == null)
                {
                    UiAction.AddJsCode("alert('Image does not contain a valid human face. Please upload a clear image of a person.');");
                    result.AddError("EmployeeId", "Image does not contain a valid human face. Please upload a clear image of a person.");
                    result.IsSuccess = false;
                    return result;
                }
                else
                {
                    Vector vector = new Vector(embeddings);
                    entity.Embedding = vector;
                }
            }
            catch (Exception ex)
            {
                UiAction.AddJsCode(ex.Message.ToString());
                result.AddError(ex.Message.ToString());
                result.IsSuccess = false;
                return result;
            }

            result.IsSuccess = true;
            return result;
        }

        private async Task<Result> EmployeeImageFormView_OnBeforeUpdate(EmployeeImage entity, IFormFileCollection files, string parentId)
        {
            var result = new Result(); // Create a new result

            try
            {
                FaceService faceService = CoreServices.GetService<FaceService>();
                var embeddings = await faceService.GetEmbeddings(files[0]);

                if (embeddings == null)
                {
                    UiAction.AddJsCode("alert('Image does not contain a valid human face. Please upload a clear image of a person.');");
                    result.AddError("EmployeeId", "Image does not contain a valid human face. Please upload a clear image of a person.");
                    result.IsSuccess = false;
                    return result;
                }
                else
                {
                    Vector vector = new Vector(embeddings);
                    entity.Embedding = vector;
                }
            }
            catch (Exception ex)
            {
                UiAction.AddJsCode(ex.Message.ToString());
                result.AddError(ex.Message.ToString());
                result.IsSuccess = false;
                return result;
            }

            result.IsSuccess = true;
            return result;
        }
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
