using System.Collections.Generic;

namespace InternProject.Core.Properties
{
    public class FilterDescriptor
    {
        public string Member { get; set; } = null!;
        public object Value { get; set; } = null!;
        public FilterOperator Operator { get; set; }
    }

    public enum FilterCompositionLogicalOperator
    {
        And,
        Or
    }
    public class CompositeFilterDescriptor
    {
        public List<FilterDescriptor> FilterDescriptors { get; set; } = new List<FilterDescriptor>();
        public FilterCompositionLogicalOperator Operator { get; set; }
    }



    public enum FilterOperator
    {
        IsLessThan,
        IsLessThanOrEqualTo,
        IsEqualTo,
        IsNotEqualTo,
        IsGreaterThanOrEqualTo,
        IsGreaterThan,
        StartsWith,
        EndsWith,
        Contains,
        DoesNotContain,
        IsContainedIn,
        IsNotContainedIn,
        IsNull,
        IsNotNull,
        IsEmpty,
        IsNotEmpty
    }
}