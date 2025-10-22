using CV_3_project.Models;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace CV_3_project.Repositories
{
    public class Counter
    {
        [BsonId]
        public string Id { get; set; }
        public int SequenceValue { get; set; }
    }

    public class MongoShiftRepository : IShiftRepository
    {
        private readonly IMongoCollection<Shift> _shifts;
        private readonly IMongoCollection<Counter> _counters;

        public MongoShiftRepository(IMongoDatabase database)
        {
            _shifts = database.GetCollection<Shift>("shifts");
            _counters = database.GetCollection<Counter>("counters");
        }

        private int GetNextSequenceValue(string sequenceName)
        {
            var filter = Builders<Counter>.Filter.Eq(c => c.Id, sequenceName);
            var update = Builders<Counter>.Update.Inc(c => c.SequenceValue, 1);
            var options = new FindOneAndUpdateOptions<Counter>
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            };

            var counter = _counters.FindOneAndUpdate(filter, update, options);
            return counter.SequenceValue;
        }

        public void Add(Shift shift)
        {
            shift.Id = GetNextSequenceValue("shiftId");

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