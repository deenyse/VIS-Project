namespace CV_3_project.Models
{
    public class Worker : Account
    {
        public Worker(string login, string password, string name, string surname, ContactInfo contacts)
            : base(login, password, name, surname, contacts) { }
    }
}