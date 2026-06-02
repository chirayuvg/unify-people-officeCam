namespace UnifyNow.Forest.Data.SQL
{
    public class Images
    {
        public Guid id { get; set; }         // Unique ID
        public string Title { get; set; }   // Title of Image
        public string faceImage { get; set; } // Base64 string (used before saving)
        public string ImageUrl { get; set; } // Local File Path of the Image

    }
}
