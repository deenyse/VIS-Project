using CV_3_project.Models;
using MongoDB.Driver;

namespace CV_3_project.Repositories
{
    public class MongoNotificationRepository : INotificationRepository
    {
        private readonly IMongoCollection<Notification> _notifications;
        private readonly IMongoCollection<Counter> _counters;

        public MongoNotificationRepository(IMongoDatabase database)
        {
            _notifications = database.GetCollection<Notification>("notifications");
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

        public void Add(Notification notification)
        {
            notification.Id = GetNextSequenceValue("notificationId");
            _notifications.InsertOne(notification);
        }

        public List<Notification> GetByUserId(int userId)
        {
            return _notifications.Find(n => n.UserId == userId).SortBy(n => n.CreatedAt).ToList();
        }

        public void DeleteByUserId(int userId)
        {
            _notifications.DeleteMany(n => n.UserId == userId);
        }
    }
}