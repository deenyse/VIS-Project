namespace CV_3_project.Models
{
    public class TimeRange
    {
        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }

        public TimeRange(DateTime startTime, DateTime endTime)
        {
            // Invariant (validation check)
            if (endTime <= startTime)
            {
                throw new ArgumentException("End time must be after start time.");
            }
            StartTime = startTime;
            EndTime = endTime;
        }

        public override string ToString()
        {
            return $"{StartTime} to {EndTime}";
        }
    }
}