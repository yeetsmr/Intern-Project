using InternProject.Core.Enums;
using InternProject.Core.Filters;


namespace InternProject.Core.Properties
{
    public class FilterDto
    {
        public List<SortRule> Sorts { get; set; } = new List<SortRule>();
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 12;

        public StringFilter? TaskName { get; set; }
        public NumericFilter<double>? MaxTime { get; set; }

        public EnumFilter<Priority>? pri { get; set; }
        public BooleanFilter? IsCompleted { get; set; }
        public NumericFilter<DateTime>? CreatedAfter { get; set; }
    }
}
