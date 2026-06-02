namespace Unify.CCC.Models
{
    public class SegmentModel
    {
        public string Polygon { get; set; }
        public List<SegmentMetaDataModel>? SegmentMetaDataModels { get; set; } = new();
    }
}
