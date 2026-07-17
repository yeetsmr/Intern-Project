
namespace InternProject.Core.Model
{
    public class CompositeFilterDescriptor
    {
        public List<FilterDescriptor> FilterDescriptors { get; set; } = new List<FilterDescriptor>();
        public FilterCompositionLogicalOperator Operator { get; set; }
    }
}
