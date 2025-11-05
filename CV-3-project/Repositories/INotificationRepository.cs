using CV_3_project.Models;

namespace CV_3_project.Repositories
{
    public interface INotificationRepository
    {
        void Add(Notification notification);
        List<Notification> GetByUserId(int userId);
        void DeleteByUserId(int userId); // Deletes all notifications for a user
    }
}