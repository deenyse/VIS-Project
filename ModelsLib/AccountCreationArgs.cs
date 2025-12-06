namespace CV_3_project.Models
{
    public class AccountCreationArgs
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public ContactInfo Contacts { get; set; }
        public string? Position { get; set; }
    }
}
