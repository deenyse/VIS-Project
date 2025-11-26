using MongoDB.Bson.Serialization.Attributes;

namespace CV_3_project.Models
{
    [BsonDiscriminator(Required = true, RootClass = true)]
    [BsonKnownTypes(typeof(Manager), typeof(Worker), typeof(GuestAccount), typeof(UnknownWorker), typeof(AppManager))]
    //[BsonKnownTypes(typeof(Manager), typeof(Worker), typeof(GuestAccount), typeof(UnknownWorker))]
    public abstract class Account : BaseEntity
    {

        public string Login { get; set; }
        //public string Password { get; set; }
        public string PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public ContactInfo Contacts { get; set; }
        public UserSettings Settings { get; set; }

        protected Account(string login, string name, string surname, ContactInfo contacts)
        {
            Login = login;
            Name = name;
            Surname = surname;
            Contacts = contacts;
            Settings = new UserSettings();
        }

        public void SetPassword(string password)
        {
            PasswordSalt = Security.SecurityHelper.GenerateSalt();
            PasswordHash = Security.SecurityHelper.HashPassword(password, PasswordSalt);
        }
        public override void Validate()
        {
            base.Validate(); // Clears the list
            if (string.IsNullOrWhiteSpace(Login))
                ValidationErrors.Add("Login is required.");

            if (string.IsNullOrWhiteSpace(Name))
                ValidationErrors.Add("Name is required.");
            if (string.IsNullOrWhiteSpace(Surname))
                ValidationErrors.Add("Surname is required.");
        }
    }
}