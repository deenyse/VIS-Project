namespace CV_3_project.Models
{
    public class Shift : BaseEntity
    {

        public TimeRange Period { get; private set; }

        public int? AssignedWorkerId { get; set; } = null;

        public Shift() { }

        public Shift(TimeRange period)
        {
            Period = period;
        }

        public override void Validate()
        {
            base.Validate();
            if (Period == null)
            {
                ValidationErrors.Add("Time period must be set.");
            }
            if (Period != null && (Period.EndTime - Period.StartTime).TotalHours > 24)
            {
                ValidationErrors.Add("Shift cannot be longer than 24 hours.");
            }
        }
    }
}