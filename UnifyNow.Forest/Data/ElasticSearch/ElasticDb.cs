using AppStudio.Framework.ElasticSearch;

namespace UnifyNow.Forest.Data.Elasticsearch
{
    public class ElasticDb : ESDbContext
    {
        public List<ForestIndex> ForestIndex { get; set; }
    }
}
    