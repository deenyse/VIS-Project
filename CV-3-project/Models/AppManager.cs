using MongoDB.Bson.Serialization.Attributes;

namespace CV_3_project.Models
{
    [BsonDiscriminator("AppManager")]
    public class AppManager : Account
    {
        public AppManager(string login, string name, string surname, ContactInfo contacts)
            : base(login, name, surname, contacts)
        {
        }
    }
}
