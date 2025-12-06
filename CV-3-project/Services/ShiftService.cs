using CV_3_project.EventSystem;
using CV_3_project.Models;
using CV_3_project.Observers;
using CV_3_project.Security;

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

            _eventManager.Register(new WorkerNotifier(_unitOfWork));
            _eventManager.Register(new ManagerNotifier(_unitOfWork));
            EnsureRootAdminExists();

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

        private void Authorize(int userId, Type requiredType)
        {
            var user = GetAccountById(userId);
            if (user == null)
            {
                throw new UnauthorizedAccessException("There is no user with provided id.");
            }
            if (user.GetType() != requiredType && !user.GetType().IsSubclassOf(requiredType))
            {
                throw new UnauthorizedAccessException("You do not have permission to perform this action.");
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

        public string? AddShift(int userId, DateTime startTime, DateTime endTime)
        {
            try
            {
                Authorize(userId, typeof(Manager));
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

        public string? AssignToShift(int userId, int shiftId)
        {
            try
            {
                Authorize(userId, typeof(Worker));
                var shift = _unitOfWork.Shifts.GetById(shiftId);
                if (shift == null || shift.AssignedWorkerId != null) return "Shift unavailable.";

                bool hasConflict = _unitOfWork.Shifts.GetAll().Any(s =>
                    s.AssignedWorkerId == userId &&
                    s.Period.StartTime < shift.Period.EndTime &&
                    s.Period.EndTime > shift.Period.StartTime
                );

                if (hasConflict) return "Schedule conflict.";

                shift.AssignedWorkerId = userId;
                shift.MarkUpdated();
                _unitOfWork.Shifts.Update(shift);
                _unitOfWork.SaveChanges();

                _eventManager.Notify("ShiftAssigned", new ShiftEventData(shift, GetAccountById(userId)));
                return null;
            }
            catch (Exception ex) { return ex.Message; }
        }

        public Account? Login(string login, string password)
        {
            //Console.WriteLine($"[DEBUG] Attempting login for: {login}");
            var account = _unitOfWork.Accounts.GetByLogin(login);
            if (account != null)
            {
                //Console.WriteLine($"[DEBUG] Account found: {account.Login}");
                if (SecurityHelper.VerifyPassword(password, account.PasswordHash, account.PasswordSalt))
                {
                    //Console.WriteLine("[DEBUG] Password verified.");
                    return account;
                }
                else
                {
                    //Console.WriteLine("[DEBUG] Password verification failed.");
                }
            }
            else
            {
                //Console.WriteLine("[DEBUG] Account not found.");
            }
            return null;
        }

        public List<Notification> GetAndClearNotifications(int userId)
        {
            var notifications = _unitOfWork.Notifications.GetByUserId(userId);
            if (notifications.Any())
            {
                _unitOfWork.Notifications.DeleteByUserId(userId);
                _unitOfWork.SaveChanges();
            }
            return notifications;
        }

        public void AddNotification(int userId, string message)
        {
            try
            {
                Authorize(userId, typeof(Manager));
                var notification = new Notification(userId, message);
                notification.Validate();
                if (notification.IsValid)
                {
                    _unitOfWork.Notifications.Add(notification);
                    _unitOfWork.SaveChanges();
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }

        public List<Shift> GetAssignedShifts() =>
            _unitOfWork.Shifts.GetAll().Where(s => s.AssignedWorkerId != null).ToList();


        public List<Shift> GetAvailableShifts() =>
           _unitOfWork.Shifts.GetAll().Where(s => s.AssignedWorkerId == null).ToList();

        public List<AssignedShiftDto> GetAssignedShiftsWithWorkers()
        {
            var shifts = GetAssignedShifts();

            return shifts.Select(s => new AssignedShiftDto
            {
                Shift = s,
                Worker = GetAccountById(s.AssignedWorkerId!.Value)
            }).ToList();
        }

        public string? CreateManager(int userId, string login, string password, string name, string surname, string email, string phone)
        {
            try
            {
                Authorize(userId, typeof(AppManager));

                var args = new AccountCreationArgs
                {
                    Login = login,
                    Password = password,
                    Name = name,
                    Surname = surname,
                    Contacts = new ContactInfo(email, phone)
                };
                return CreateAccount(AccountType.Manager, args);
            }
            catch (Exception ex) { return ex.Message; }
        }

        public string? CreateWorker(int userId, string login, string password, string name, string surname, string email, string phone, string position)
        {
            try
            {
                Authorize(userId, typeof(AppManager));

                var args = new AccountCreationArgs
                {
                    Login = login,
                    Password = password,
                    Name = name,
                    Surname = surname,
                    Contacts = new ContactInfo(email, phone),
                    Position = position
                };
                return CreateAccount(AccountType.Worker, args);
            }
            catch (Exception ex) { return ex.Message; }
        }
        private Account? GetAccountById(int id)
        {
            return _unitOfWork.Accounts.GetById(id);
        }
    }
}