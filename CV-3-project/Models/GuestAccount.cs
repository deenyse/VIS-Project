using MongoDB.Bson.Serialization.Attributes;

namespace CV_3_project.Models
{
    [BsonDiscriminator("GuestAccount")]
    public class GuestAccount : Account
    {
        public GuestAccount()
            : base("guest", "Guest", "", new ContactInfo("", ""))
        {
        }

        public override void Validate()
        {
            ValidationErrors.Clear();
        }
    }
}