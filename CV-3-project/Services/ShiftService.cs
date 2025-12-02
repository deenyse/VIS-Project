using CV_3_project.EventSystem;
using CV_3_project.Models;
using CV_3_project.Observers;

namespace CV_3_project.Services
{
    public class ShiftService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AccountFactory _accountFactory;
        private readonly EventManager _eventManager;

        public ShiftService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _accountFactory = new AccountFactory();
            _eventManager = new EventManager();

            // Подключаем уведомления
            _eventManager.Register(new WorkerNotifier(_unitOfWork));
            _eventManager.Register(new ManagerNotifier(_unitOfWork));
        }

        public void EnsureRootAdminExists()
        {
            var adminExists = _unitOfWork.Accounts.GetAll().OfType<AppManager>().Any();
            if (!adminExists)
            {
                var args = new AccountCreationArgs
                {
                    Login = "admin",
                    Password = "admin",
                    Name = "Root",
                    Surname = "Admin",
                    Contacts = new ContactInfo("root.root", "0")
                };
                CreateAccount(AccountType.AppManager, args);
            }
        }

        public string? CreateAccount(AccountType type, AccountCreationArgs args)
        {
            try
            {
                var account = _accountFactory.CreateAccount(type, args);
                account.Validate();
                if (!account.IsValid) return string.Join(", ", account.ValidationErrors);

                _unitOfWork.Accounts.Add(account);
                _unitOfWork.SaveChanges();
                return null;
            }
            catch (Exception ex) { return ex.Message; }
        }

        public string? AddShift(DateTime startTime, DateTime endTime)
        {
            try
            {
                var period = new TimeRange(startTime, endTime);
                var shift = new Shift(period);
                shift.Validate();
                if (!shift.IsValid) return string.Join(", ", shift.ValidationErrors);

                _unitOfWork.Shifts.Add(shift);
                _unitOfWork.SaveChanges();

                _eventManager.Notify("NewShift", new ShiftEventData(shift, null));
                return null;
            }
            catch (Exception ex) { return ex.Message; }
        }

        public string? AssignToShift(int shiftId, int workerId)
        {
            try
            {
                var shift = _unitOfWork.Shifts.GetById(shiftId);
                if (shift == null || shift.AssignedWorkerId != null) return "Shift unavailable.";

                bool hasConflict = _unitOfWork.Shifts.GetAll().Any(s =>
                    s.AssignedWorkerId == workerId &&
                    s.Period.StartTime < shift.Period.EndTime &&
                    s.Period.EndTime > shift.Period.StartTime
                );

                if (hasConflict) return "Schedule conflict.";

                shift.AssignedWorkerId = workerId;
                shift.MarkUpdated();
                _unitOfWork.Shifts.Update(shift);
                _unitOfWork.SaveChanges();

                var worker = _unitOfWork.Accounts.GetById(workerId);
                _eventManager.Notify("ShiftAssigned", new ShiftEventData(shift, worker));
                return null;
            }
            catch (Exception ex) { return ex.Message; }
        }

        public List<Shift> GetAvailableShifts() =>
            _unitOfWork.Shifts.GetAll().Where(s => s.AssignedWorkerId == null).ToList();
    }
}