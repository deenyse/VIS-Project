using CV_3_project.Repositories;

namespace CV_3_project
{
    public interface IUnitOfWork : IDisposable
    {
        IAccountRepository Accounts { get; }
        IShiftRepository Shifts { get; }
        INotificationRepository Notifications { get; }
        void SaveChanges();
    }
}