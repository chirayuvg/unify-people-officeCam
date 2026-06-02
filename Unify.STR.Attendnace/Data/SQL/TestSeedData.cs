using AppStudio.Enterprise.Data;
using AppStudio.Enterprise.Services;
using AppStudio.Framework.Core;
using AppStudio.Framework.EntityFramework.Extensions;
using AppStudio.Model;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using System;
using System.Linq;
using System.Threading.Tasks;
using Unify.STR.Attendance.Data.SQL;

namespace Unify.STR.Attendance.Data.SQL
{
    public class TestSeedData : SeedData
    {

        public override async Task SeedDataAsync()
        {
            var context = CoreServices.GetService<PeopleDb>();

            Tenant tenant = new Tenant
            {
                Id = GuidHelper.EncodeStringToGuid("DTC"),
                Name = "DTC",
                IsActive = true,
                TenantType = TenantType.Company
            };

            await Entity.UpsertAsync(tenant, m => m.Id == tenant.Id);


            TenantUser tenantUser = new()
            {
                Id = Guid.NewGuid(),
                Name = "Admin",
                Email = "admin@dtc.com",
                Password = "1234",
                IsActive = true,
                TenantId = tenant.Id,
            };
            await Entity.UpsertAsync(tenantUser, m => m.TenantId == tenantUser.TenantId);

            Tenant tenant1 = new Tenant
            {
                Id = GuidHelper.EncodeStringToGuid("STR"),
                Name = "STR",
                IsActive = true,
                TenantType = TenantType.Company
            };
            await Entity.UpsertAsync(tenant1, m => m.Id == tenant1.Id);


            TenantUser tenantUser1 = new()
            {
                Id = Guid.NewGuid(),
                Name = "STR",
                Email = "admin@str.com",
                Password = "Str@123",
                IsActive = true,
                TenantId = tenant1.Id,
            };
            await Entity.UpsertAsync(tenantUser1, m => m.TenantId == tenantUser1.TenantId);


        }
    }
}