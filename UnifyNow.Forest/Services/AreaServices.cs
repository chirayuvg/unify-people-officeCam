using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using UnifyNow.Forest.Data.SQL;

namespace UnifyNow.Forest.Services
{
    public class AreaServices
    {
        private readonly ForestDb _forestDb;

        public AreaServices(IDbContextFactory<ForestDb> dbContextFactory)
        {
            _forestDb = dbContextFactory.CreateDbContext();
        }

        public async Task<List<AreaRequest>> GetArea()
        {
            try
            {
                var aoi = await _forestDb.Areas
               .Select(t => new {t.Id, t.Name, t.Polygon,t.CameraId })
               .ToListAsync();
                var response = aoi.Select(t => new AreaRequest
                {
                    Id = t.Id.ToString(),
                    Name = t.Name,
                    Polygon = t.Polygon,
                    CameraId = t.CameraId.ToString()
                }).ToList();
                return response;
            }
            catch
            {
                throw;
            }
        }
        public async Task<List<AreaRequest>> GetAreaById(string CameraId)
        {
            try
            {
                var aoi = await _forestDb.Areas
               .Select(t => new { t.Id, t.Name, t.Polygon, t.CameraId })
               .Where(m=> m.CameraId == Guid.Parse(CameraId)).ToListAsync();
                var response = aoi.Select(t => new AreaRequest
                {
                    Id = t.Id.ToString(),
                    Name = t.Name,
                    Polygon = t.Polygon,
                    CameraId = t.CameraId.ToString()
                }).ToList();
                return response;
            }
            catch
            {
                throw;
            }
        }

        public async Task<IResult> AddArea(AreaRequest areas)
        {
            try
            {
                Area area = new();
                area.Id = Guid.NewGuid();
                area.Name = areas.Name;
                area.Polygon = areas.Polygon;
                area.MapPolygon= areas.Polygon;
                area.CameraId = Guid.Parse(areas.CameraId);
                await _forestDb.Areas.AddAsync(area);
                await _forestDb.SaveChangesAsync();
                return Results.Ok();
            }
            catch
            {
                throw;
            }
        }
        public async Task<IResult> UpdateArea(AreaRequest areas)
        {
            try
            {
                var data = _forestDb.Areas.Find(Guid.Parse(areas.Id));
                if(areas.Name != "")
                {
                    data.Name = areas.Name;
                }
                else if(areas.Polygon != "")
                {
                    data.Polygon = areas.Polygon;
                    data.MapPolygon = areas.Polygon;
                }
                _forestDb.Areas.Update(data);
                _forestDb.SaveChangesAsync();
                return Results.Ok(data);
            }
            catch
            {
                throw;
            }
        }
        public class AreaRequest
        {
            public string? Id { get; set; }  
            public string? Name { get; set; }
            public string? Polygon { get; set; }

            public string? CameraId { get; set; }
        }
    }

}

