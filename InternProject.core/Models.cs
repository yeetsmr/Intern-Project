using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace InternProject.Core
{

    public enum Priorty
    {
        low,
        mid,
        high
    }

    public class task
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string TaskName { get; set; } = null!;
        public double MaxTime { get; set; }
        public bool IsCompleted { get; set; }
        public Priorty pri { get; set; }
        public DateTime CreatedAfter { get; set; }
    }

    public class FilterDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;

        public string? TaskNameContains { get; set; }
        public double? MaxTime { get; set; }
        public int? pri { get; set; }
        public bool? IsCompleted { get; set; }
    }
    public class LoginDto
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}