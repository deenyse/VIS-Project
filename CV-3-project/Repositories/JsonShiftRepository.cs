namespace CV_3_project.Repositories
{
    public class JsonShiftRepository : IShiftRepository
    {
        private readonly List<Shift> _shifts;

        // Constructor now accepts a list from Unit of Work
        public JsonShiftRepository(List<Shift> shifts)
        {
            _shifts = shifts;
        }

        public void Add(Shift shift)
        {
            shift.Id = _shifts.Any() ? _shifts.Max(s => s.Id) + 1 : 1;
            _shifts.Add(shift);
            // SaveChanges() is removed
        }

        public List<Shift> GetAll()
        {
            return _shifts;
        }

        public Shift? GetById(int id)
        {
            return _shifts.FirstOrDefault(s => s.Id == id);
        }

        public void Update(Shift shift)
        {
            var existingShift = GetById(shift.Id);
            if (existingShift != null)
            {
                existingShift.AssignedWorkerLogin = shift.AssignedWorkerLogin;
                // SaveChanges() is removed
            }
        }
    }
}