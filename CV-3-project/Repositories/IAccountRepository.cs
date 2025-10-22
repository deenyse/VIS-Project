using CV_3_project.Models;

namespace CV_3_project.Repositories
{
    public interface IAccountRepository
    {
        void Add(Account account);
        Account? GetByLogin(string login);
        Account? GetById(int id);
        List<Account> GetAll();
    }
}