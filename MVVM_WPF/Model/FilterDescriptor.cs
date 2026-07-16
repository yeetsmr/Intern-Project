using MVVM_WPF.Model.Enums;

namespace MVVM_WPF.Model
{
    public class FilterDescriptor
    {
        public string? Member { get; set; }
        public object? Value { get; set; }
        public FilterOperator Operator { get; set; }
    }
}