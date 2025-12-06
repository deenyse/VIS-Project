namespace CV_3_project.Models
{
    public class ContactInfo
    {
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        public ContactInfo(string email, string phoneNumber)
        {
            Email = email;
            PhoneNumber = phoneNumber;
        }
    }
}