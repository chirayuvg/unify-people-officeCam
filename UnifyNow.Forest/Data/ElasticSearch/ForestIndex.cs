namespace UnifyNow.Forest.Data.Elasticsearch
{
    //public class AnimalLogIndex
    //{
    //    public string Id { get; set; }
    //    public DateTime DateTime { get; set; }

    //    public string AnimalId { get; set; }
    //    public string Animal {  get; set; }

    //    public string AreaId { get; set; }
    //    public string Area { get; set; }
    //    public string Longitude { get; set; }

    //    public string Latitude { get; set; }

    //    public string AOI { get; set; }
    //    public string? FileURL { get; set; }

    //    public Guid CustomerId { get; set; }
    //}
    public class ForestIndex
    {
        public string Id { get; set; }
        public Guid? CameraId { get; set; }
        public string CameraName { get; set; }
        public Guid IntelligenceId { get; set; }
        public string IntelligenceName { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime TimeStamp { get; set; }
        public string FileURL { get; set; }
        public Guid CustomerId { get; set; }  
        
        public string AnimalName { get; set; }

        public string PersonName { get; set; }

        public string Alert { get; set; }
        public string TrackingId { get; set; }
        public string BoundingBox { get; set; }    
        
        public string? AreaId { get; set; }

        public string? AreaName { get; set; }

        public string? PresetId { get; set; }
        
        public string? PresetName { get; set; }
        
    }
}
