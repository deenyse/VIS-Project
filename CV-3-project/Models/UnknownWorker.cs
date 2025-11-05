using MongoDB.Bson.Serialization.Attributes;

namespace CV_3_project.Models
{
    [BsonDiscriminator("UnknownWorker")]
    public class UnknownWorker : Worker
    {
        public UnknownWorker()
            : base("unknown", "", "Unknown", "Worker", new ContactInfo("", ""), "N/A")
        {
        }
    }
}