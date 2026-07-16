using System.Collections.Generic;
using MVVM_WPF.Model.Enums;

namespace MVVM_WPF.Model
{
    public class CompositeFilterDescriptor
    {
        public List<FilterDescriptor> FilterDescriptors { get; set; } = new List<FilterDescriptor>();
        public FilterCompositionLogicalOperator LogicalOperator { get; set; }
    }
}