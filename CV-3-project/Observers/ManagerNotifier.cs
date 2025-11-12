using CV_3_project.EventSystem;
using CV_3_project.Models;

namespace CV_3_project.Observers
{
    public class ManagerNotifier : IObserver
    {
        private readonly IUnitOfWork _unitOfWork;

        public ManagerNotifier(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public void Update(string eventType, EventData? data)
        {
            if (eventType != "ShiftAssigned")
                return;

            if (data is not ShiftEventData shiftData)
                return;

            var shift = shiftData.Shift;
            var account = shiftData.Account;

            if (account == null) return;

            var managers = _unitOfWork.Accounts.GetAll().OfType<Manager>();
            foreach (var manager in managers)
            {
                var notification = new Notification(manager.Id, $"Worker {account.Name} assigned to shift {shift.Id}");
                notification.Validate();
                if (notification.IsValid)
                {
                    _unitOfWork.Notifications.Add(notification);
                }
            }
            _unitOfWork.SaveChanges();
        }
    }
}
