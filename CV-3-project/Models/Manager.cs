using MongoDB.Bson.Serialization.Attributes;

namespace CV_3_project.Models
{

    [BsonDiscriminator("Manager")]
    public class Manager : Account
    {
        public Manager(string login, string name, string surname, ContactInfo contacts)
            : base(login, name, surname, contacts) { }
    }
}