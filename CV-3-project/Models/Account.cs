using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CV_3_project.Models
{
    [BsonDiscriminator(Required = true)]
    [BsonKnownTypes(typeof(Manager), typeof(Worker))]
    public abstract class Account
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Login { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }

        public Account() { }

        public Account(string login, string password, string name, string surname)
        {
            Login = login;
            Password = password;
            Name = name;
            Surname = surname;
        }
    }
}