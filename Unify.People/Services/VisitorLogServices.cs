using AppStudio.Framework.Core;
using DTC.Unify.HttpClientService;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DynamicLinq;
using Newtonsoft.Json;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using Unify.Data;
using Unify.Data.SQL;
using Unify.People.Controllers.Visitors;
using Unify.People.RulesEngine;

namespace Unify.People.Services
{
    public class VisitorLogServices
    {
        private readonly PeopleDb _dbContext;
        private readonly IWebHostEnvironment _env;
        private readonly FaceService _faceService;

        public VisitorLogServices(IDbContextFactory<PeopleDb> dbContextFactory, FaceService faceService, IWebHostEnvironment env)
        {
            _dbContext = dbContextFactory.CreateDbContext();
            _env = env;
            _faceService = faceService;
        }

        public async Task<Guid?> GetVisitorId(float[] embeddings)
        {
            try
            {
                Vector capturedVector = new Vector(embeddings);

                var items = await _dbContext.VisitorImages
                    .Select(m => new { Entity = m, Distance = m.Embedding!.CosineDistance(capturedVector) })
                    .Where(m => m.Distance < 0.50)    // 75%
                    //.Where(m => m.Distance < 0.25)    // >87%
                    //.Where(m => m.Distance < 0.10)    // 95%
                    .OrderBy(m => m.Distance)
                    .Select(m => new { m.Entity.VisitorId, m.Distance, m.Entity.Embedding })
                    .Take(3)
                    .ToListAsync();

                bool allIdsSame = items.Select(m => m.VisitorId).Distinct().Count() == 1;

                if (allIdsSame)
                {
                    var firstItem = items.FirstOrDefault();
                    var cosineSimilarityResult = EmbeddingComparer.CosineSimilarity(
                        embeddings.Select(f => (double)f).ToArray()
                        , firstItem.Embedding.ToArray().Select(f => (double)f).ToArray()
                    );

                    if (cosineSimilarityResult >= 0.5)
                        return items.FirstOrDefault()?.VisitorId;
                    else
                        return null;
                }

                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task ValidateImage(CheckInModel checkInModel)
        {
            try
            {
                var peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();
                var tenentId = CoreServices.CurrentUser.GetTenantId();
                Guid? visitorId = null;

                var embedding = await _faceService.GetEmbeddings(checkInModel.Image);
                if (embedding != null && embedding.Length != 0) visitorId = await GetVisitorId(embedding);

                //  If Visitor not found, return to create a new Visitor
                if (visitorId == null) return;

                //  If Visitor found, create CheckInLog
                Vector vector = new Vector(embedding);
                Visitor visitor = await Entity.Query<Visitor>().FirstOrDefaultAsync(m => m.Id == visitorId);

                //  Fill up with the Visitor Information
                checkInModel.VisitorId = visitor.Id;
                checkInModel.VisitorName = visitor.Name;
                checkInModel.MobileNo = visitor.MobileNo;
                checkInModel.Company = visitor.Company;
                checkInModel.Address = visitor.Address;
                checkInModel.Email = visitor.Email;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<string> InsertVisitor(CheckInModel checkInModel)
        {
            string returnValue = "";

            try
            {
                var peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();
                var tenentId = CoreServices.CurrentUser.GetTenantId();

                var embedding = await _faceService.GetEmbeddings(checkInModel.Image);
                if (embedding == null || embedding.Length == 0) return "Image has not captured properly. Please try again.";
                Vector vector = new Vector(embedding);

                //  Create Visitor
                Visitor visitor = new Visitor();
                visitor.Name = checkInModel.VisitorName;
                visitor.MobileNo = checkInModel.MobileNo;
                visitor.Email = checkInModel.Email;
                visitor.Company = checkInModel.Company;
                visitor.Address = checkInModel.Address;
                visitor.TenantId = tenentId;
                var visitorRep = CoreServices.GetRepository<Visitor>();
                await visitorRep.InsertAsync(visitor);

                //  Create Visitor Image Record
                VisitorImage visitorImage = new VisitorImage();
                visitorImage.Image = "Photo.PNG";
                visitorImage.VisitorId = visitor.Id;
                visitorImage.TenantId = tenentId;
                visitorImage.Embedding = vector;
                var visitorImageRep = CoreServices.GetRepository<VisitorImage>();
                await visitorImageRep.InsertAsync(visitorImage);

                //  Create Visitor Image File
                var base64Data = checkInModel.ImageBase64String.Contains(",") ? checkInModel.ImageBase64String.Split(',')[1] : checkInModel.ImageBase64String;
                byte[] imageBytes = Convert.FromBase64String(base64Data);
                var stream = new MemoryStream(imageBytes);
                IFileStorageService fileStorage = CoreServices.FileStorageService;
                string pathiImg = Path.Combine(nameof(VisitorImage), nameof(VisitorImage.Image));
                await fileStorage.SaveFileAsync("documents", pathiImg, visitorImage.Id.ToString(), MimeHelper.GetMimeType("jpg"), stream);

                //  Create CheckInLog
                await InsertVisitorLog(checkInModel, visitor, vector);
            }
            catch (Exception ex)
            {
                returnValue = ex.Message.ToString();
                throw;
            }

            return returnValue;
        }

        public async Task<string> UpdateVisitor(CheckInModel checkInModel)
        {
            string returnValue = "";

            try
            {
                //  Update Visitor
                Visitor visitor = await Entity.Query<Visitor>().FirstOrDefaultAsync(m => m.Id == checkInModel.VisitorId);
                visitor.Name = checkInModel.VisitorName;
                visitor.MobileNo = checkInModel.MobileNo;
                visitor.Email = checkInModel.Email;
                visitor.Company = checkInModel.Company;
                visitor.Address = checkInModel.Address;

                await Entity.UpdateAsync(checkInModel.VisitorId, visitor);

                //  Create CheckInLog
                var embedding = await _faceService.GetEmbeddings(checkInModel.Image);
                if (embedding == null || embedding.Length == 0) return "Image has not captured properly. Please try again.";
                Vector vector = new Vector(embedding);

                var peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();
                var ifVisitorLogExist = peopleDb.VisitorLogs
                    .Where(m => m.AppointmentId == checkInModel.AppointmentId && m.VisitorId == visitor.Id)
                    .FirstOrDefault();

                if (ifVisitorLogExist == null)
                {
                    var ifBadgeNumberExist = peopleDb.VisitorLogs
                    .Where(m => m.AppointmentId == checkInModel.AppointmentId && m.BadgeNo == checkInModel.BadgeNo)
                        .FirstOrDefault();

                    if (ifBadgeNumberExist == null)
                    {
                        await InsertVisitorLog(checkInModel, visitor, vector);
                    }
                    else
                    {
                        returnValue = "DUPLICATE_BADGE_NO";
                    }
                }
                else
                {
                    returnValue = "DUPLICATE_VISITOR";
                }
            }
            catch (Exception ex)
            {
                returnValue = ex.Message.ToString();
                throw;
            }

            return returnValue;
        }

        private async Task InsertVisitorLog(CheckInModel checkInModel, Visitor visitor, Vector vector)
        {
            var tenentId = CoreServices.CurrentUser.GetTenantId();

            var peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();
            var appointment = peopleDb.Appointments
                .Where(m => m.Id == checkInModel.AppointmentId)
                .Include(m => m.MeetingPurpose)
                .Include(m => m.MeetingPlace)
                .SingleOrDefault();

            var employeeRep = CoreServices.GetRepository<Unify.Data.SQL.Employee>();
            var employee = await employeeRep.FindByIdAsync(appointment.EmployeeId);

            VisitorLog visitorLog = new()
            {
                Id = Guid.NewGuid(),
                DateTime = DateTime.UtcNow.AddHours(5).AddMinutes(30),
                TimeStamp = DateTime.UtcNow.AddHours(5).AddMinutes(30),

                TenantId = tenentId,
                IntelligenceId = Guid.Parse("AE2EE380-56D6-4C9F-8DDB-08DD23E0CD5A"),
                CameraId = Guid.Parse("3CD15053-3022-4C11-969A-4ED98D253B55"),
                AppointmentId = checkInModel.AppointmentId,
                VisitorId = visitor.Id,
                EmployeeId = employee.Id,
                MeetingPurposeId = appointment.MeetingPurposeId,
                MeetingPlaceId = appointment.MeetingPlaceId,

                IntelligenceName = "Visitor",
                CameraName = "Vis",
                Notes = appointment.Notes,
                
                VisitorName = visitor.Name,
                MobileNo = visitor.MobileNo,
                Company = visitor.Company,
                Address = visitor.Address,
                Email = visitor.Email,

                EmployeeCode = employee.Code,
                EmployeeName = employee.Name,

                MeetingPurpose = appointment.MeetingPurpose.Name,
                MeetingPlace = appointment.MeetingPlace.Name,

                AppointmentDate = appointment.AppointmentDate,
                AppointmentTime = appointment.AppointmentTime,
                MeetingDate = DateOnly.FromDateTime(DateTime.Now),
                StartTime = TimeOnly.FromDateTime(DateTime.Now).AddHours(5).AddMinutes(30),
                EndTime = TimeOnly.MinValue,

                BadgeNo = checkInModel.BadgeNo,

                Image = "Image.png",
                Embedding = vector,
                Latitude = checkInModel.Latitude,
                Longitude = checkInModel.Longitude,
            };

            var files = new FormFileCollection
            {
                checkInModel.Image
            };

            var res = await Entity.InsertAsync(visitorLog);

            checkInModel.CheckInLogId = visitorLog.Id;
            checkInModel.VisitorId = visitor.Id;
            checkInModel.VisitorName = visitor.Name;
            checkInModel.MobileNo = visitor.MobileNo;
            checkInModel.Company = visitor.Company;
            checkInModel.Address = visitor.Address;
            checkInModel.Email = visitor.Email;
        }

        public async Task<Guid> UpdateVisitorLog(Guid Id, string values)
        {
            //  Update VisitorLog
            VisitorLog visitorLog = await Entity.Query<VisitorLog>().FirstOrDefaultAsync(m => m.Id == Id);
            JsonConvert.PopulateObject(values, visitorLog);
            Guid AppointmentId = visitorLog.AppointmentId;
            await Entity.UpdateAsync(Id, visitorLog);

            //  Update Visitor
            if (values.Contains("visitorName") || values.Contains("company") || values.Contains("address"))
            {
                Visitor visitor = await Entity.Query<Visitor>().FirstOrDefaultAsync(m => m.Id == visitorLog.VisitorId);
                visitor.Name = visitorLog.VisitorName;
                visitor.Company = visitorLog.Company;
                visitor.Address = visitorLog.Address;
                await Entity.UpdateAsync(visitor.Id, visitor);
            }

            return AppointmentId;
        }

        public async Task<Guid> DeleteVisitorLog(Guid Id)
        {
            VisitorLog visitorLog = await Entity.Query<VisitorLog>().FirstOrDefaultAsync(m => m.Id == Id);
            Guid AppointmentId = visitorLog.AppointmentId;
            await Entity.DeleteAsync<VisitorLog, Guid>(Id);
            return AppointmentId;
        }

        public async Task<List<VisitorLog>> GetVisitorLogByAppointmentIdAsync(Guid AppointmentId)
        {
            var peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();
            var visitorLogs = peopleDb.VisitorLogs
                .Where(m => m.AppointmentId == AppointmentId)
                .OrderBy(m => m.StartTime)
                .ToList();

            return visitorLogs;
        }
        public async Task<List<VisitorLog>> GetVisitorLogByAppointmentIdVisitorIdAsync(Guid AppointmentId, Guid VisitorId)
        {
            var peopleDb = AppStudio.Framework.Core.CoreServices.GetService<PeopleDb>();
            var visitorLogs = peopleDb.VisitorLogs
    .Where(m => m.AppointmentId == AppointmentId)
    .GroupBy(m => m.VisitorId)
    .Select(g => g
        .OrderByDescending(x => x.AppointmentDate)
        .ThenByDescending(x => x.AppointmentTime).FirstOrDefault()
        )
    .ToList();

            //var visitorLogs = peopleDb.VisitorLogs
            //    .Where(m => m.AppointmentId == AppointmentId && m.VisitorId == VisitorId)
            //    .OrderByDescending(m=>m.AppointmentDate).ThenByDescending(m=>m.AppointmentTime).Distinct()
            //    .ToList();

            return visitorLogs;
        }

    }
}
