using System.Text.Json.Serialization;

namespace CV_3_project.Models
{
    [JsonDerivedType(typeof(Manager), typeDiscriminator: "manager")]
    [JsonDerivedType(typeof(Worker), typeDiscriminator: "worker")]
    public class Account
    {
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