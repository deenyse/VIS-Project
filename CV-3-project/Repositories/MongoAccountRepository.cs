﻿using CV_3_project.Models;
using MongoDB.Driver;

namespace CV_3_project.Repositories
{
    public class MongoAccountRepository : IAccountRepository
    {
        private readonly IMongoCollection<Account> _accounts;
        private readonly IMongoCollection<Counter> _counters; // Добавляем коллекцию счетчиков

        public MongoAccountRepository(IMongoDatabase database)
        {
            _accounts = database.GetCollection<Account>("accounts");
            _counters = database.GetCollection<Counter>("counters"); // Получаем доступ к коллекции счетчиков
        }

        // Безопасный метод для получения следующего ID
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

        public void Add(Account account)
        {
            if (_accounts.Find(a => a.Login == account.Login).Any())
            {
                throw new System.Exception("Account with this login already exists.");
            }

            // 1. Получаем новый уникальный ID из счетчика
            account.Id = GetNextSequenceValue("accountId");

            // 2. Вставляем аккаунт с уже присвоенным ручным ID
            _accounts.InsertOne(account);
        }

        public List<Account> GetAll()
        {
            return _accounts.Find(account => true).ToList();
        }

        public Account? GetByLogin(string login)
        {
            return _accounts.Find(a => a.Login == login).FirstOrDefault();
        }

        public Account? GetById(int id)
        {
            return _accounts.Find(a => a.Id == id).FirstOrDefault();
        }
    }
}