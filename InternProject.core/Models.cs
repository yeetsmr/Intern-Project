using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace InternProject.Core
{

    public enum Priority
    {
        low,
        mid,
        high
    }

    public class Tasks
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string TaskName { get; set; } = null!;
        public double MaxTime { get; set; }
        public bool IsCompleted { get; set; }
        public Priority pri { get; set; }
        public DateTime CreatedAfter { get; set; }
    }

    public class FilterDto
    {
        public List<SortRule> Sorts { get; set; } = new List<SortRule>();
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 12;

        public StringFilter? TaskName { get; set; }
        public NumericFilter? MaxTime { get; set; }

        public EnumFilter<Priority>? pri { get; set; }

        public BooleanFilter? IsCompleted { get; set; }
        public DateFilter? CreatedAfter { get; set; }

    }
    public class StringFilter
    {
        public string Value { get; set; } = null!;
        public string MatchMode { get; set; } = "Contains";
    }


    public class NumericFilter
    {
        public double? Min { get; set; }
        public double? Max { get; set; }
    }

    public class DateFilter
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }


    public class EnumFilter<T> where T : struct, Enum
    {
        public List<T> SelectedValues { get; set; } = new List<T>();
    }

    public class BooleanFilter
    {
        public bool Value { get; set; }
    }
    public class LoginDto
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
    public class SortRule
    {
        public string Member { get; set; } = null!;
        public string SortDirection { get; set; } = null!;
    }
}
