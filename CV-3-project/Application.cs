using CV_3_project.EventSystem;  // <-- Добавлено
using CV_3_project.Models;
using CV_3_project.Observers;    // <-- Добавлено

namespace CV_3_project
{
    class Application
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AccountFactory _accountFactory;
        private readonly EventManager _eventManager;

        public Account signedInUser { get; private set; } = new GuestAccount();

        public Application(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _accountFactory = new AccountFactory();
            _eventManager = new EventManager();
        }

        public void RegisterObserver(IObserver observer)
        {
            _eventManager.Register(observer);
        }

        public void UnregisterObserver(IObserver observer)
        {
            _eventManager.Unregister(observer);
        }

        private bool AddAccountInternal(AccountType type, AccountCreationArgs args)
        {
            try
            {
                var account = _accountFactory.CreateAccount(type, args);

                account.Validate();
                if (!account.IsValid)
                {
                    Console.WriteLine("Validation failed:");
                    foreach (var error in account.ValidationErrors)
                        Console.WriteLine($" - {error}");
                    return false;
                }

                _unitOfWork.Accounts.Add(account);
                _unitOfWork.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating account: {ex.Message}");
                return false;
            }
        }

        public bool AddManager(string login, string password, string name, string surname, ContactInfo contacts)
        {
            var args = new AccountCreationArgs
            {
                Login = login,
                Password = password,
                Name = name,
                Surname = surname,
                Contacts = contacts
            };
            return AddAccountInternal(AccountType.Manager, args);
        }

        public bool AddWorker(string login, string password, string name, string surname, ContactInfo contacts, string position)
        {
            var args = new AccountCreationArgs
            {
                Login = login,
                Password = password,
                Name = name,
                Surname = surname,
                Contacts = contacts,
                Position = position
            };
            return AddAccountInternal(AccountType.Worker, args);
        }

        public bool AddShift(DateTime startTime, DateTime endTime)
        {
            if (!IsManager())
                return false;

            try
            {
                var period = new TimeRange(startTime, endTime);
                var shift = new Shift(period);

                shift.Validate();
                if (!shift.IsValid)
                {
                    Console.WriteLine("Shift validation failed:");
                    foreach (var error in shift.ValidationErrors)
                        Console.WriteLine($" - {error}");
                    return false;
                }

                _unitOfWork.Shifts.Add(shift);
                _unitOfWork.SaveChanges();


                var eventData = new ShiftEventData(shift, null);
                _eventManager.Notify("NewShift", eventData);

                return true;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Error creating shift: {ex.Message}");
                return false;
            }
        }

        public bool AssignToShift(int shiftId)
        {
            if (signedInUser is GuestAccount) return false;

            var shift = _unitOfWork.Shifts.GetById(shiftId);
            if (shift == null || shift.AssignedWorkerId != null)
                return false;

            bool hasConflict = _unitOfWork.Shifts.GetAll().Any(s =>
                s.AssignedWorkerId == signedInUser.Id &&
                s.Period.StartTime < shift.Period.EndTime &&
                s.Period.EndTime > shift.Period.StartTime
            );

            if (hasConflict)
                return false;

            shift.AssignedWorkerId = signedInUser.Id;
            shift.MarkUpdated();
            _unitOfWork.Shifts.Update(shift);
            _unitOfWork.SaveChanges();


            var eventData = new ShiftEventData(shift, signedInUser);
            _eventManager.Notify("ShiftAssigned", eventData);

            return true;
        }

        public bool Login(string login, string password)
        {
            var account = _unitOfWork.Accounts.GetByLogin(login);
            if (account != null && account.Password == password)
            {
                signedInUser = account;
                return true;
            }
            return false;
        }

        public void Logout()
        {
            signedInUser = new GuestAccount();
        }

        public bool IsManager()
        {
            return signedInUser is Manager;
        }

        public List<Shift> GetAvailableShifts()
        {
            return _unitOfWork.Shifts.GetAll().Where(s => s.AssignedWorkerId == null).ToList();
        }

        public List<(Shift shift, Account worker)> GetAssignedShiftsWithWorker()
        {
            var assignedShifts = _unitOfWork.Shifts.GetAll()
                .Where(s => s.AssignedWorkerId != null)
                .ToList();

            var workers = _unitOfWork.Accounts.GetAll()
                .ToDictionary(a => a.Id, a => a);

            return assignedShifts.Select(s =>
            {
                Account? worker = workers.GetValueOrDefault(s.AssignedWorkerId!.Value);
                if (worker == null)
                    worker = new UnknownWorker();

                return (s, worker);
            }).ToList();
        }

        public void AddNotification(int userId, string message)
        {
            var notification = new Notification(userId, message);
            notification.Validate();
            if (notification.IsValid)
            {
                _unitOfWork.Notifications.Add(notification);
                _unitOfWork.SaveChanges();
            }
        }

        public List<Notification> GetAndClearUserNotifications(int userId)
        {
            var notifications = _unitOfWork.Notifications.GetByUserId(userId);
            if (notifications.Any())
            {
                _unitOfWork.Notifications.DeleteByUserId(userId);
                _unitOfWork.SaveChanges();
            }
            return notifications;
        }
    }
}