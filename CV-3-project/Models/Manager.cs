namespace CV_3_project.Models
{
    public class Manager : Account
    {
        public Manager() { }

        public Manager(string login, string password, string name, string surname)
            : base(login, password, name, surname) { }
    }
}