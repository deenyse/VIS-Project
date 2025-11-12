using CV_3_project.EventSystem;
using CV_3_project.Models;

namespace CV_3_project.Observers
{
    public class WorkerNotifier : IObserver
    {
        private readonly IUnitOfWork _unitOfWork;

        public WorkerNotifier(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public void Update(string eventType, EventData? data)
        {
            if (eventType != "NewShift")
                return;

            if (data is not ShiftEventData shiftData)
                return;

            var shift = shiftData.Shift;

            var workers = _unitOfWork.Accounts.GetAll().OfType<Worker>();
            foreach (var worker in workers)
            {
                var notification = new Notification(worker.Id, $"New shift added: {shift.Period}");
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
