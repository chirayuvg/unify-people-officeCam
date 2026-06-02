namespace UnifyNow.Forest.Data.SQL
{
    public class AlertLog
    {
        public string Id { get; set; }
        public DateTime DateTime { get; set; }

        public string Animal { get; set; }

        public string Longitude { get; set; }

        public string Latitude { get; set; }

        public string AOI { get; set; }
        public string? FileURL { get; set; }

        public Guid? AlertId { get; set; }
        public Alert? Alert { get; set; }

        public Guid CustomerId { get; set; }
    }
}
