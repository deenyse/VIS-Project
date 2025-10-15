using CV_3_project.Models;

namespace CV_3_project.Repositories
{
    public class JsonAccountRepository : IAccountRepository
    {
        private readonly List<Account> _accounts;

        // Constructor now accepts a list from Unit of Work
        public JsonAccountRepository(List<Account> accounts)
        {
            _accounts = accounts;
        }

        public void Add(Account account)
        {
            if (_accounts.Any(a => a.Login == account.Login))
            {
                throw new System.Exception("Account with this login already exists.");
            }
            _accounts.Add(account);
            // SaveChanges() is removed
        }

        public List<Account> GetAll()
        {
            return _accounts;
        }

        public Account? GetByLogin(string login)
        {
            return _accounts.FirstOrDefault(a => a.Login == login);
        }
    }
}