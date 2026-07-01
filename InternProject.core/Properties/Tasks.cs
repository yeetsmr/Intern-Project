using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace InternProject.Core.Properties
{
    public class Tasks
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string TaskName { get; set; } = null!;
        public string AssigneeName { get; set; } = null!;

        public double MaxTime { get; set; }
        public double EstimatedCost { get; set; }

        public bool IsCompleted { get; set; }
        public bool IsActive { get; set; }

        public Priority pri { get; set; }
        public Category? Category { get; set; }

        public DateTime CreatedAfter { get; set; }
        public DateTime DueDate { get; set; }

        public int StoryPoints { get; set; }
        public int SprintNumber { get; set; }
    }
}
