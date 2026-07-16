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
}