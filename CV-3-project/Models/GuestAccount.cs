using MongoDB.Bson.Serialization.Attributes;

namespace CV_3_project.Models
{
    [BsonDiscriminator("GuestAccount")]
    public class GuestAccount : Account
    {
        public GuestAccount()
            : base("guest", "", "Guest", "", new ContactInfo("", ""))
        {
            // Guest account is always valid in the system context
        }

        public override void Validate()
        {
            // Guest does not require validation
            ValidationErrors.Clear();
        }
    }
}