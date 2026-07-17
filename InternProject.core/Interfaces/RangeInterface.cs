using System;
using System.Collections.Generic;
using System.Text;

namespace InternProject.Core.Interfaces 
{
    public interface RangeInterface
    {
        void SetMin(object value);
        void SetMax(object value);
        object? GetMin();
        object? GetMax();
    }
}
