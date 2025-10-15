using System;

namespace CV_3_project
{
    public class Shift
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? AssignedWorkerLogin { get; set; } = null;

        public Shift() { }

        public Shift(DateTime startTime, DateTime endTime)
        {
            StartTime = startTime;
            EndTime = endTime;
        }
    }
}