using MongoDB.Bson.Serialization.Attributes;

namespace CV_3_project.Models
{
    public abstract class BaseEntity
    {
        // 1. Primary Identifier
        public int Id { get; set; }

        // 2. Auditing
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        // 3. Validation
        [BsonIgnore] // Don't save errors to the DB
        public List<string> ValidationErrors { get; private set; } = new List<string>();

        [BsonIgnore]
        public bool IsValid => ValidationErrors.Count == 0;

        public BaseEntity()
        {
            CreatedAt = DateTime.UtcNow;
            MarkUpdated();
        }

        // 4. Method to update timestamp
        public void MarkUpdated()
        {
            UpdatedAt = DateTime.UtcNow;
        }

        // 5. Virtual validation method
        public virtual void Validate()
        {
            ValidationErrors.Clear();
        }
    }
}
