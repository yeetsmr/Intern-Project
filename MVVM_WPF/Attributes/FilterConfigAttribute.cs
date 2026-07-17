using MVVM_WPF.Model.Enums;

namespace MVVM_WPF.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FilterConfigAttribute : Attribute
    {
        public string Member { get; }
        public FilterOperator Operator { get; }

        public FilterConfigAttribute(string member, FilterOperator op)
        {
            Member = member;
            Operator = op;
        }
    }
}