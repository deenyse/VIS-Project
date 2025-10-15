namespace CV_3_project.Models
{
    public class Worker : Account
    {
        public Worker() { }

        public Worker(string login, string password, string name, string surname)
            : base(login, password, name, surname) { }
    }
}