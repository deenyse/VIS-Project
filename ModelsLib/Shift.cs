namespace CV_3_project.Models
{
    public class Shift : BaseEntity // <-- Mod: Inherits from BaseEntity
    {
        // public int Id { get; set; } // <-- Mod: Removed (now in BaseEntity)

        // <-- Mod: Replaced with VO TimeRange
        // public DateTime StartTime { get; set; }
        // public DateTime EndTime { get; set; }
        public TimeRange Period { get; private set; }

        public int? AssignedWorkerId { get; set; } = null;

        public Shift() { }

        // <-- Mod: Constructor updated for VO
        public Shift(TimeRange period)
        {
            Period = period;
        }

        // <-- Mod: Added validation method
        public override void Validate()
        {
            base.Validate();
            if (Period == null)
            {
                ValidationErrors.Add("Time period must be set.");
            }
            // Additional validation: shift cannot be longer than 24 hours
            if (Period != null && (Period.EndTime - Period.StartTime).TotalHours > 24)
            {
                ValidationErrors.Add("Shift cannot be longer than 24 hours.");
            }
        }
    }
}