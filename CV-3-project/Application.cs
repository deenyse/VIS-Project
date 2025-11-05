using CV_3_project.Models;

namespace CV_3_project
{
    class Application
    {
        private readonly IUnitOfWork _unitOfWork;
        // <-- Mod: Initialize SC GuestAccount
        public Account signedInUser { get; private set; } = new GuestAccount();

        public Application(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public bool AddManager(string login, string password, string name, string surname, ContactInfo contacts)
        {
            var account = new Manager(login, password, name, surname, contacts);
            // <-- Mod: Call LS validation
            account.Validate();
            if (!account.IsValid)
            {
                Console.WriteLine("Validation failed:");
                foreach (var error in account.ValidationErrors)
                    Console.WriteLine($" - {error}");
                return false;
            }

            try
            {
                _unitOfWork.Accounts.Add(account);
                _unitOfWork.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }

        public bool AddWorker(string login, string password, string name, string surname, ContactInfo contacts, string position)
        {
            var account = new Worker(login, password, name, surname, contacts, position);
            // <-- Mod: Call LS validation
            account.Validate();
            if (!account.IsValid)
            {
                Console.WriteLine("Validation failed:");
                foreach (var error in account.ValidationErrors)
                    Console.WriteLine($" - {error}");
                return false;
            }

            try
            {
                _unitOfWork.Accounts.Add(account);
                _unitOfWork.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }

        public bool AddShift(DateTime startTime, DateTime endTime)
        {
            if (!IsManager())
                return false;

            try
            {
                // <-- Mod: Create VO
                var period = new TimeRange(startTime, endTime);
                var shift = new Shift(period);

                // <-- Mod: Call LS validation
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

                // <-- Mod: Send notifications to all workers
                var workers = _unitOfWork.Accounts.GetAll().OfType<Worker>();
                foreach (var worker in workers)
                {
                    AddNotification(worker.Id, $"New shift added: {shift.Period}");
                }

                return true;
            }
            catch (ArgumentException ex) // <-- Mod: Catch VO invariant error
            {
                Console.WriteLine($"Error creating shift: {ex.Message}");
                return false;
            }
        }

        public bool AssignToShift(int shiftId)
        {
            if (signedInUser is GuestAccount) return false; // <-- Mod: Check for SC

            var shift = _unitOfWork.Shifts.GetById(shiftId);
            if (shift == null || shift.AssignedWorkerId != null)
                return false;

            bool hasConflict = _unitOfWork.Shifts.GetAll().Any(s =>
                s.AssignedWorkerId == signedInUser.Id &&
                s.Period.StartTime < shift.Period.EndTime && // <-- Mod: Using VO
                s.Period.EndTime > shift.Period.StartTime   // <-- Mod: Using VO
            );

            if (hasConflict)
                return false;

            shift.AssignedWorkerId = signedInUser.Id;
            shift.MarkUpdated(); // <-- Mod: Call LS
            _unitOfWork.Shifts.Update(shift);
            _unitOfWork.SaveChanges();

            // <-- Mod: Notify manager
            var managers = _unitOfWork.Accounts.GetAll().OfType<Manager>();
            foreach (var manager in managers)
            {
                AddNotification(manager.Id, $"Worker {signedInUser.Name} assigned to shift {shift.Id}");
            }

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
            signedInUser = new GuestAccount(); // <-- Mod: Set SC
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
                    worker = new UnknownWorker(); // <-- Mod: Use SC

                return (s, worker);
            }).ToList();
        }

        // <-- Mod: New methods for notification system

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