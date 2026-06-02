using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using UnifyNow.Forest.Data.SQL;
using UnifyNow.Forest.HttpClientService;
using static UnifyNow.Forest.Services.AreaServices;

namespace UnifyNow.Forest.Services
{
    public class PresetServices
    {
        private readonly ForestDb _forestDb;
        private readonly IMapper _mapper;
        private readonly AIForestService _aIForestService;
        
        public PresetServices(IDbContextFactory<ForestDb> dbContextFactory, IMapper mapper,AIForestService aIForestService)
        {
            _forestDb = dbContextFactory.CreateDbContext();
            _mapper = mapper;
            _aIForestService = aIForestService;
        }

        public async Task<List<PresetModel>> GetPreset()
        {
            try
            {
                var preset = await _forestDb.Presets.OrderByDescending(m => m.CreatedOn).Select(preset => _mapper.Map<PresetModel>(preset)).ToListAsync();
                return preset;
            }
            catch
            {
                throw;
            }
        }
        public async Task<List<PresetModel>> GetPresetById(string AreaId)
        {
            try
            {
                var preset = await _forestDb.Presets.Where(m => m.AreaId == Guid.Parse(AreaId)).OrderByDescending(m => m.CreatedOn).Select(preset => _mapper.Map<PresetModel>(preset)).ToListAsync();
                return preset;
            }
            catch
            {
                throw;
            }
        }
        public async Task<List<PresetModel>> GetPresetByCameraId(string CameraId)
        {
            try
            {
                var preset = await _forestDb.Presets.Where(m => m.CameraId == Guid.Parse(CameraId)).OrderByDescending(m => m.CreatedOn).Select(preset => _mapper.Map<PresetModel>(preset)).ToListAsync();
                return preset;
            }
            catch
            {
                throw;
            }
        }
        public async Task<IResult> AddPreset(PresetCreate preset)
        {
            try
            {

                PresetAIModel presetAIModel = new();
                presetAIModel.presetId = Guid.NewGuid().ToString();
                presetAIModel.presetName = preset.PresetName;
                presetAIModel.cameraName = preset.CameraName;
                presetAIModel.cameraId = preset.CameraId;
                presetAIModel.areaName = preset.AreaName;
                presetAIModel.duration = preset.Duration;
                var res = await _aIForestService.AddPresetAsync(presetAIModel);
                if (res != null)
                {
                    Preset presets = new();
                    presets.Id = Guid.Parse(presetAIModel.presetId);
                    presets.Name = presetAIModel.presetName;
                    presets.CameraId = Guid.Parse(preset.CameraId);
                    presets.Pan = res.data.pan.ToString();
                    presets.Tilt = res.data.tilt.ToString();
                    presets.Zoom = res.data.zoom.ToString();
                    presets.Duration = preset.Duration;
                    presets.CreatedOn = DateTime.UtcNow.AddMinutes(330);
                    presets.AreaId = Guid.Parse(preset.AreaId);
                    await _forestDb.Presets.AddAsync(presets);
                    await _forestDb.SaveChangesAsync();
                    return Results.Ok();
                }
                else
                {
                    return Results.BadRequest();
                }
            }
            catch
            {
                throw;
            }
        }
    }
    public class PresetModel
    {
       
        public string Id { get; set; } 

        public string Name { get; set; }

        public string? CameraId { get; set; }

        public int Duration { get; set; }
        public decimal Pan { get; set; }

        public decimal Tilt { get; set; }

        public decimal Zoom { get; set; }

        public string AreaId { get; set; }

        public DateTime CreatedOn { get; set; }
    }

    public class PresetCreate
    {
        public string PresetName { get; set; }

        public string CameraName { get; set; }
        public string CameraId { get; set; }
        public string AreaId { get; set; }

        public string AreaName { get; set; }
        public int Duration { get; set; }
   
    }


}
