using Microsoft.EntityFrameworkCore;
using UnifyNow.Forest.Data.SQL;

namespace UnifyNow.Forest.Services
{
    public class CameraServices
    {
        private readonly ForestDb _forestDb;

        public CameraServices(IDbContextFactory<ForestDb> dbContextFactory)
        {
            _forestDb = dbContextFactory.CreateDbContext();
        }

        public async Task<List<CameraModel>> GetCameraConfigurationsAsync(Guid customerId)
        {
            List<CameraModel> cameraModels = new List<CameraModel>();

            //
            List<MonitorModel> monitorModels = new();
           

            var monitors = await _forestDb.Monitors
                .Include(m => m.Intelligence).Include(m => m.Camera).Where(m => m.CustomerId == customerId).ToListAsync();



            foreach (var monitor in monitors)
            {
                CameraModel cameraModel = new CameraModel();

                //var camera = cameras.Where(m => m.Id  == data.CameraId).SingleOrDefault();
                //var segment = segments.Where(m => m.CameraId == data.CameraId).AsEnumerable();
                cameraModel.CustomerId = monitor.CustomerId;
                cameraModel.CameraId = monitor.Camera.Id;
                cameraModel.Name = monitor.Camera.Name;
                cameraModel.Description = monitor.Camera.Description;
                cameraModel.CameraProtocol = monitor.Camera.CameraProtocol.ToString();
                cameraModel.ConnectionString = monitor.Camera.ConnectionString;
                cameraModel.AOI = "";



                MonitorModel monitorModel = new();
                monitorModel.MonitorFrequency = monitor.MonitorFrequency;
                IntelligenceModel intelligence = new IntelligenceModel();
                intelligence.IntelligenceId = monitor.Intelligence.Id;
                intelligence.Name = monitor.Intelligence.Name;
                intelligence.MonitorType = monitor.Intelligence.MonitorType.ToString();

                monitorModel.IntelligenceModel = intelligence;
                // validate
                //public List<MonitorModel>? MonitorModel { get; set; }
                cameraModel.MonitorModels.Add(monitorModel);

                cameraModels.Add(cameraModel);
            }
            return cameraModels;
        }
    }
    public class CameraModel
    {
        public Guid CameraId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CameraProtocol { get; set; }
        public string ConnectionString { get; set; } = string.Empty;
        public string? AOI { get; set; }
        
        public List<MonitorModel>? MonitorModels { get; set; } = new();
        
        public Guid CustomerId { get; set; }
    }
    
    public class MonitorModel
    {
        public IntelligenceModel? IntelligenceModel { get; set; }
        public int MonitorFrequency { get; set; }
    }
   
    public class IntelligenceModel
    {
        public Guid IntelligenceId { get; set; }
        public string Name { get; set; }
        public string MonitorType { get; set; }
    }
    public enum MonitorType
    {
        Manual,
        RealTime,
        Schedule
    }
    public enum CameraProtocol
    {
        RTSP,
        USB
    }
}
