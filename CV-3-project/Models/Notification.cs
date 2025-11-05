namespace CV_3_project.Models
{
    public class Notification : BaseEntity
    {
        public int UserId { get; set; } // Recipient's ID
        public string Message { get; set; }

        // Constructor for simplicity
        public Notification(int userId, string message)
        {
            UserId = userId;
            Message = message;
        }

        public override void Validate()
        {
            base.Validate();
            if (UserId <= 0)
                ValidationErrors.Add("Valid UserId is required.");
            if (string.IsNullOrWhiteSpace(Message))
                ValidationErrors.Add("Message cannot be empty.");
        }
    }
}