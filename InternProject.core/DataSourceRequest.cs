using System.Text.Json.Serialization;
using InternProject.Core.Model;



namespace InternProject.Core
{
    public class DataSourceRequest
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        [JsonPropertyName("filter")]
        public CompositeFilterDescriptor Filter { get; set; } = null!;

        [System.Text.Json.Serialization.JsonPropertyName("sorts")]
        public List<SortRule> Sorts { get; set; } = new List<SortRule>();

    }
  
}