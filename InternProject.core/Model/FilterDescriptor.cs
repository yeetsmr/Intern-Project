using InternProject.Core.Enums;

namespace InternProject.Core.Model
{
    public class FilterDescriptor
    {
        public string Member { get; set; } = null!;
        public object Value { get; set; } = null!;
        public FilterOperator Operator { get; set; }
    }
}
