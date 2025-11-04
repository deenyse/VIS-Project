using MongoDB.Bson.Serialization.Attributes;

namespace CV_3_project.Models
{
    // Атрибуты для STI:
    // BsonDiscriminator: Говорит, что это базовый класс иерархии
    // BsonKnownTypes: Перечисляет все возможные классы-потомки
    [BsonDiscriminator(Required = true, RootClass = true)]
    [BsonKnownTypes(typeof(Manager), typeof(Worker))]
    public abstract class Account
    {
        public int Id { get; set; }

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