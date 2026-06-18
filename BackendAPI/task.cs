using InternProject.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace InternProject.Models
{
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
}