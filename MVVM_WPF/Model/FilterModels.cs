using System.Collections.Generic;

namespace MVVM_WPF.Model
{
    public class DataSourceRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public CompositeFilterDescriptor Filter { get; set; } = new CompositeFilterDescriptor();
        public List<SortDescriptor> Sorts { get; set; } = new List<SortDescriptor>();
    }

    public class CompositeFilterDescriptor
    {
        public List<FilterDescriptor> FilterDescriptors { get; set; } = new List<FilterDescriptor>();
        public FilterCompositionLogicalOperator LogicalOperator { get; set; }
    }

    public class FilterDescriptor
    {
        public string Member { get; set; }
        public object Value { get; set; }
        public FilterOperator Operator { get; set; }
    }

    public enum FilterCompositionLogicalOperator { And, Or }

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

    public class SortDescriptor
    {
        public string Member { get; set; }
        public string SortDirection { get; set; }
    }

    public class LoginResponse
    {
        public string Token { get; set; }
    }
}