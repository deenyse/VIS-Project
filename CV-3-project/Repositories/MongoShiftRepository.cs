using CV_3_project.Models;
using MongoDB.Driver;

namespace CV_3_project.Repositories
{
    public class MongoShiftRepository : IShiftRepository
    {
        private readonly IMongoCollection<Shift> _shifts;

        public MongoShiftRepository(IMongoDatabase database)
        {
            _shifts = database.GetCollection<Shift>("shifts");
        }

        public void Add(Shift shift)
        {
            // Get the last max Id and increment it
            var maxId = _shifts.Find(s => true).SortByDescending(s => s.Id).Limit(1).FirstOrDefault()?.Id ?? 0;
            shift.Id = maxId + 1;
            _shifts.InsertOne(shift);
        }

        public List<Shift> GetAll()
        {
            return _shifts.Find(shift => true).ToList();
        }

        public Shift? GetById(int id)
        {
            return _shifts.Find(s => s.Id == id).FirstOrDefault();
        }

        public void Update(Shift shift)
        {
            _shifts.ReplaceOne(s => s.Id == shift.Id, shift);
        }
    }
}