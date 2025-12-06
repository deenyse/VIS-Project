namespace ClientTerminal
{
    public class Shift
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public TimeRange Period { get; set; }
    }

    public class TimeRange
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public override string ToString()
        {
            return $"{StartTime} - {EndTime}";
        }
    }

    public class Notification
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Message { get; set; }
    }

    public class AssignedShiftDto
    {
        public Shift Shift { get; set; }
        public Worker Worker { get; set; }
    }

    public record ContactInfo(string Email, string Phone);

    public class Account
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
    }

    public class GuestAccount : Account { }
    public class AppManager : Account { }
    public class Manager : Account { }
    public class Worker : Account
    {
        public string Position { get; set; }
    }

}
