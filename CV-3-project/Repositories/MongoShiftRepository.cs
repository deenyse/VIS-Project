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
            // Ручная генерация ID больше не нужна
            _shifts.InsertOne(shift);
        }

        public List<Shift> GetAll()
        {
            return _shifts.Find(shift => true).ToList();
        }

        public Shift? GetById(string mongoId)
        {
            return _shifts.Find(s => s.MongoId == mongoId).FirstOrDefault();
        }

        public void Update(Shift shift)
        {
            // Используем MongoId для поиска документа для замены
            _shifts.ReplaceOne(s => s.MongoId == shift.MongoId, shift);
        }
    }
}