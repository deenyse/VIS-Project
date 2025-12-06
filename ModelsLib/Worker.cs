using MongoDB.Bson.Serialization.Attributes;

namespace CV_3_project.Models
{

    [BsonDiscriminator("Worker")]

    public class Worker : Account
    {
        public string position { get; set; }

        public Worker(string login, string name, string surname, ContactInfo contacts, string position)
            : base(login, name, surname, contacts)
        {

            this.position = position;
        }
    }
}