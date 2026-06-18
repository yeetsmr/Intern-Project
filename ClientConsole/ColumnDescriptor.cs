using System;

namespace ClientConsole
{
    public class ColumnDescriptor
    {
        public string? ColumnName { get; set; }
        public string? DisplayName { get; set; }
        public Type DataType { get; set; }
        public string? SelectedOperator { get; set; }
        public object FilterValue { get; set; }
    }
}