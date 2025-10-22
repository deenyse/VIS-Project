using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CV_3_project.Models
{
    public class Shift
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string MongoId { get; set; }

        // Целочисленный ID больше не нужен, MongoId - наш уникальный ключ
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        // Паттерн "Foreign Key Mapping" - ссылка на MongoId работника
        [BsonRepresentation(BsonType.ObjectId)]
        public string? AssignedWorkerId { get; set; } = null;

        public Shift(DateTime startTime, DateTime endTime)
        {
            StartTime = startTime;
            EndTime = endTime;
        }
    }
}