using InternProject.Models;
using System;
namespace InternProject.Models
{
    public class FilterDto
    {
        public string? TaskNameContains { get; set; }
        public double? MaxTime { get; set; } 
        public int? pri { get; set; }        
        public bool? IsCompleted { get; set; }
    }
}