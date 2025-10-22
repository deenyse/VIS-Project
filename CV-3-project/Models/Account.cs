using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CV_3_project.Models
{
    [BsonKnownTypes(typeof(Manager), typeof(Worker))]
    public abstract class Account
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string MongoId { get; set; }

        public string Login { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }

        public ContactInfo Contacts { get; set; }

        public UserSettings Settings { get; set; }

        protected Account(string login, string password, string name, string surname, ContactInfo contacts)
        {
            Login = login;
            Password = password;
            Name = name;
            Surname = surname;
            Contacts = contacts;
            Settings = new UserSettings();
        }
    }
}