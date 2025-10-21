using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CV_3_project.Models
{
    public class Shift
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string MongoId { get; set; }

        // Existing integer Id for application logic
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? AssignedWorkerLogin { get; set; } = null;

        public Shift() { }

        public Shift(DateTime startTime, DateTime endTime)
        {
            StartTime = startTime;
            EndTime = endTime;
        }
    }
}