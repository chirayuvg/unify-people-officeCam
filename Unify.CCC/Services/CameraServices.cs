using Microsoft.EntityFrameworkCore;

using Unify.CCC.Data;
using Microsoft.CodeAnalysis;
using Unify.CCC.Models;
using System.Threading;
using Monitor = Unify.CCC.Data.Monitor;
using AppStudio.Framework.Core;

namespace Unify.CCC.Services
{
    public class CameraServices
    {
        private readonly CCCDb _cccdb;
       // private readonly Guid _customerId;
        
        public CameraServices(IDbContextFactory<CCCDb> dbContextFactory)
        {
            _cccdb = dbContextFactory.CreateDbContext();
            // _customerId = CoreServices.CurrentUser.GetCustomerId();

        }

        public async Task<List<CameraModel>> GetCameraConfigurationsAsync()
        {
            List<CameraModel> cameraModels = new List<CameraModel>();

            //
            List<CameraMetaDataModel> cameraMetaDataModels = new();
            List<MonitorModel> monitorModels = new();
            List<AlertModel> alertModels = new();
            List<SegmentModel> segmentModels = new();

            var monitors = await _cccdb.Monitors
                .Include(m => m.Intelligence).ThenInclude(m => m.Alerts)
                .Include(m => m.Camera).ThenInclude(m => m.CameraMetaData)
                .ToListAsync()
                ;

            //var cameras = await _cccdb.Cameras.Include(m => m.CameraMetaData).ToListAsync();
            var segments = await _cccdb.Segments.Include(m => m.SegmentMetaData).ToListAsync();

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
                cameraModel.AOI = monitor.Camera.AOI;

                foreach (var data in monitor.Camera.CameraMetaData)
                {
                    CameraMetaDataModel cameraMetaDataModel = new CameraMetaDataModel();
                    cameraMetaDataModel.Key = data.Key;
                    cameraMetaDataModel.Value = data.Value;
                    //public List<CameraMetaDataModel>? CameraMetaDataModel { get; set; }
                    cameraModel.CameraMetaDataModels.Add(cameraMetaDataModel);
                }
                //.CameraMetaDataModel = monitor.Camera.cam

                MonitorModel monitorModel = new();
                monitorModel.MonitorFrequency = monitor.MonitorFrequency;
                IntelligenceModel intelligence = new IntelligenceModel();
                intelligence.IntelligenceId = monitor.Intelligence.Id;
                intelligence.Name = monitor.Intelligence.Name;
                intelligence.MonitorType = monitor.Intelligence.MonitorType.ToString();
               
                foreach (var data in monitor.Intelligence.Alerts)
                {
                    // reset models
                    AlertModel alert = new AlertModel();
                    alert.Name = data.Name;
                    //alert.Value = data.Value;
                    intelligence.Alerts.Add(alert) ;
                }
                monitorModel.IntelligenceModel = intelligence;
                // validate
                //public List<MonitorModel>? MonitorModel { get; set; }
                cameraModel.MonitorModels.Add(monitorModel);

                var cameraSegments = segments.Where(m => m.CameraId == cameraModel.CameraId).ToList();

                foreach (var data in cameraSegments)
                {
                    // reset models
                    SegmentModel segmentModel = new SegmentModel();
                    segmentModel.Polygon = data.Polygon;
                    foreach (var det in data.SegmentMetaData)
                    {
                        // reset models
                        SegmentMetaDataModel segmentMetaDataModel = new SegmentMetaDataModel();
                        segmentMetaDataModel.Key = det.Key;
                        segmentMetaDataModel.Value = det.Value;
                        segmentModel.SegmentMetaDataModels.Add(segmentMetaDataModel);
                    }
                    // set list of SegmentModel
                    cameraModel.SegmentsModels.Add(segmentModel);
                }
                cameraModels.Add(cameraModel);
            }
            return cameraModels;
        }

        public async Task<List<CameraModel>> GetCameraConfigurationByIdAsync(Guid cameraId)
        {
            List<CameraModel> cameraModels = new List<CameraModel>();

            //
            List<CameraMetaDataModel> cameraMetaDataModels = new();
            List<MonitorModel> monitorModels = new();
            List<AlertModel> alertModels = new();
            List<SegmentModel> segmentModels = new();

            var monitors = await _cccdb.Monitors
                .Include(m => m.Intelligence).ThenInclude(m => m.Alerts)
                .Include(m => m.Camera).ThenInclude(m => m.CameraMetaData)
                .Where(m => m.CameraId == cameraId).ToListAsync()
                ;

            //var cameras = await _cccdb.Cameras.Include(m => m.CameraMetaData).ToListAsync();
            var segments = await _cccdb.Segments.Include(m => m.SegmentMetaData)
                .Where(m => m.CameraId == cameraId)
                .ToListAsync();

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
                cameraModel.AOI = monitor.Camera.AOI;

                foreach (var data in monitor.Camera.CameraMetaData)
                {
                    CameraMetaDataModel cameraMetaDataModel = new CameraMetaDataModel();
                    cameraMetaDataModel.Key = data.Key;
                    cameraMetaDataModel.Value = data.Value;
                    //public List<CameraMetaDataModel>? CameraMetaDataModel { get; set; }
                    cameraModel.CameraMetaDataModels.Add(cameraMetaDataModel);
                }
                //.CameraMetaDataModel = monitor.Camera.cam

                MonitorModel monitorModel = new();
                monitorModel.MonitorFrequency = monitor.MonitorFrequency;
                IntelligenceModel intelligence = new IntelligenceModel();
                intelligence.IntelligenceId = monitor.Intelligence.Id;
                intelligence.Name = monitor.Intelligence.Name;
                intelligence.MonitorType = monitor.Intelligence.MonitorType.ToString();

                foreach (var data in monitor.Intelligence.Alerts)
                {
                    // reset models
                    AlertModel alert = new AlertModel();
                    alert.Name = data.Name;
                    //alert.Value = data.Value;
                    intelligence.Alerts.Add(alert);
                }
                monitorModel.IntelligenceModel = intelligence;
                // validate
                //public List<MonitorModel>? MonitorModel { get; set; }
                cameraModel.MonitorModels.Add(monitorModel);

                foreach (var data in segments)
                {
                    // reset models
                    SegmentModel segmentModel = new SegmentModel();
                    segmentModel.Polygon = data.Polygon;
                    foreach (var det in data.SegmentMetaData)
                    {
                        // reset models
                        SegmentMetaDataModel segmentMetaDataModel = new SegmentMetaDataModel();
                        segmentMetaDataModel.Key = det.Key;
                        segmentMetaDataModel.Value = det.Value;
                        segmentModel.SegmentMetaDataModels.Add(segmentMetaDataModel);
                    }
                    // set list of SegmentModel
                    cameraModel.SegmentsModels.Add(segmentModel);
                }
                cameraModels.Add(cameraModel);
            }
            return cameraModels;
        }

        public async Task<Monitor> SaveMonitorAsync(Monitor monitor)
        {
            try
            {
                _cccdb.Monitors.Add(monitor);
                await _cccdb.SaveChangesAsync();
            }
            catch
            {
                throw;
            }
            return monitor;
        }

        public async Task<Monitor> UpdateMonitorAsync(Monitor monitor)
        {
            try
            {
                var monitorData = await _cccdb.Monitors.FindAsync(monitor.Id);
                monitorData.CameraId = monitor.CameraId;
                monitorData.IntelligenceId = monitor.IntelligenceId;
                monitorData.MonitorFrequency = monitor.MonitorFrequency;

                _cccdb.Monitors.Update(monitorData);
                await _cccdb.SaveChangesAsync();
                return monitorData;
            }
            catch
            {
                throw;
            }

            return monitor;
        }

        public async Task<Camera> GetCameraByIdAsync(Guid id)
        {
            try
            {
                var camera = await _cccdb.Cameras.Where(m => m.Id == id).SingleOrDefaultAsync();

                return camera;
            }
            catch
            {
                throw;
            }
        }


    }

    //public class MonitorModel
    //{
    //    public Guid Id { get; set; }
    //    public Guid IntelligenceId { get; set; }
    //    public Intelligence? Intelligence { get; set; }
    //    public int MonitorFrequency { get; set; }
    //    public Guid CameraId { get; set; }
    //    public Camera? Camera { get; set; }
    //}
}

