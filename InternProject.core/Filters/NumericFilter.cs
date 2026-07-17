using InternProject.Core.Interfaces;

namespace InternProject.Core.Filters
{
    public class NumericFilter<T> : RangeInterface where T : struct, IComparable<T>
    {
        public T? Min { get; set; }
        public T? Max { get; set; }

        public void SetMin(object value)
        {
            if (value != null) Min = (T)value;
        }

        public void SetMax(object value)
        {
            if (value != null) Max = (T)value;
        }
        public object? GetMin() => Min;
        public object? GetMax() => Max;
    }
}
