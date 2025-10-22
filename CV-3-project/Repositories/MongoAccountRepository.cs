using CV_3_project.Models;
using MongoDB.Driver;

namespace CV_3_project.Repositories
{
    public class MongoAccountRepository : IAccountRepository
    {
        private readonly IMongoCollection<Account> _accounts;

        public MongoAccountRepository(IMongoDatabase database)
        {
            _accounts = database.GetCollection<Account>("accounts");
        }

        public void Add(Account account)
        {
            if (_accounts.Find(a => a.Login == account.Login).Any())
            {
                throw new System.Exception("Account with this login already exists.");
            }
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

        public Account? GetById(string mongoId)
        {
            return _accounts.Find(a => a.MongoId == mongoId).FirstOrDefault();
        }
    }
}