using System;
using System.Collections.Generic;
using System.Text;
using InternProject.Core.Filters;
namespace InternProject.Core.Properties

{

    public class DetailedFilterDto : FilterDto

    {

        public StringFilter? EmployeeName { get; set; }

        public NumericFilter<double>? EstimatedCost { get; set; }

        public NumericFilter<DateTime>? DueDate { get; set; }

        public EnumFilter<Category>? Category { get; set; }

        public NumericFilter<int>? StoryPoints { get; set; }

        public NumericFilter<int>? SprintNumber { get; set; }

        public BooleanFilter? IsActive { get; set; }



    }

}