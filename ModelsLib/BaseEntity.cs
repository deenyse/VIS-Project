using MongoDB.Bson.Serialization.Attributes;

namespace CV_3_project.Models
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }

        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        [BsonIgnore] // Don't save errors to the DB
        public List<string> ValidationErrors { get; private set; } = new();

        [BsonIgnore]
        public bool IsValid => ValidationErrors.Count == 0;

        public BaseEntity()
        {
            CreatedAt = DateTime.UtcNow;
            MarkUpdated();
        }

        public void MarkUpdated()
        {
            UpdatedAt = DateTime.UtcNow;
        }

        public virtual void Validate()
        {
            ValidationErrors.Clear();
        }

    }
}
