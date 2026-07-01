using System;
using System.Collections.Generic;
using System.Text;

namespace InternProject.Core.Filters
{
    public class StringFilter
    {
        public string Value { get; set; } = null!;
        public string MatchMode { get; set; } = "Contains";
    }
}
