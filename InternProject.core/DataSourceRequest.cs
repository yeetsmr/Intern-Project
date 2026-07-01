using InternProject.Core.Properties;
using System.Text.Json.Serialization;


namespace InternProject.Core
{
    public class DataSourceRequest
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        [JsonPropertyName("filter")]
        public CompositeFilterDescriptor Filter { get; set; } = null!;

        [System.Text.Json.Serialization.JsonPropertyName("sorts")]
        public List<SortDescriptor> Sorts { get; set; } = new List<SortDescriptor>();

    }
    public class SortDescriptor
    {
        public string Member { get; set; } = null!;
        public string SortDirection { get; set; } = null!;
    }
}