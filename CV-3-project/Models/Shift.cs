namespace CV_3_project.Models
{
    public class Shift
    {
        public int Id { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        // Обновляем "внешний ключ" для связи с Id аккаунта
        public int? AssignedWorkerId { get; set; } = null;

        public Shift() { }

        public Shift(DateTime startTime, DateTime endTime)
        {
            StartTime = startTime;
            EndTime = endTime;
        }
    }
}