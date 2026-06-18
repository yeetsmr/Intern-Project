using InternProject.Models;
using System;
namespace InternProject.Models
{
    public class FilterDto
    {
        public string? TaskNameContains { get; set; }
        public double? MaxEstimatedTime { get; set; }
        public bool? IsCompleted { get; set; }
        public Priorty? SelectedPriority { get; set; }
        public DateTime? CreatedAfter { get; set; }
    }
}