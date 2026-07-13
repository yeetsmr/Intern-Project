using System;
using MVVM_WPF.Model;

namespace MVVM_WPF.MVVM
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