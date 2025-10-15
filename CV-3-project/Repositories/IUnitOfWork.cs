using CV_3_project.Repositories;

namespace CV_3_project
{
    // Defines a contract for the Unit of Work
    public interface IUnitOfWork : IDisposable
    {
        IAccountRepository Accounts { get; }
        IShiftRepository Shifts { get; }
        void SaveChanges();
    }
}