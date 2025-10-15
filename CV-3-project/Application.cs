using CV_3_project.Models;

namespace CV_3_project
{
    class Application
    {
        private readonly IUnitOfWork _unitOfWork;
        public string? signedInUserLogin = null;

        // The application now depends on the Unit of Work
        public Application(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public void AddManager(string login, string password, string name, string surname)
        {
            _unitOfWork.Accounts.Add(new Manager(login, password, name, surname));
            _unitOfWork.SaveChanges(); // Commit changes
        }

        public void AddWorker(string login, string password, string name, string surname)
        {
            _unitOfWork.Accounts.Add(new Worker(login, password, name, surname));
            _unitOfWork.SaveChanges(); // Commit changes
        }

        public bool AddShift(DateTime startTime, DateTime endTime)
        {
            if (!IsManager())
                return false;

            _unitOfWork.Shifts.Add(new Shift(startTime, endTime));
            _unitOfWork.SaveChanges(); // Commit changes
            return true;
        }

        public bool AssignToShift(int shiftId)
        {
            var shift = _unitOfWork.Shifts.GetById(shiftId);
            if (shift == null || shift.AssignedWorkerLogin != null)
                return false;

            bool hasConflict = _unitOfWork.Shifts.GetAll().Any(s =>
                s.AssignedWorkerLogin == signedInUserLogin &&
                s.StartTime < shift.EndTime &&
                s.EndTime > shift.StartTime
            );

            if (hasConflict)
                return false;

            shift.AssignedWorkerLogin = signedInUserLogin;
            _unitOfWork.Shifts.Update(shift);
            _unitOfWork.SaveChanges(); // Commit all changes at the end
            return true;
        }

        public bool Login(string login, string password)
        {
            var account = _unitOfWork.Accounts.GetByLogin(login);
            if (account != null && account.Password == password)
            {
                signedInUserLogin = login;
                return true;
            }
            return false;
        }

        public void Logout()
        {
            signedInUserLogin = null;
        }

        public bool IsManager()
        {
            var account = _unitOfWork.Accounts.GetByLogin(signedInUserLogin);
            return account is Manager;
        }

        public List<Shift> GetAvailableShifts()
        {
            return _unitOfWork.Shifts.GetAll().Where(s => s.AssignedWorkerLogin == null).ToList();
        }

        public List<Shift> GetAssignedShifts()
        {
            return _unitOfWork.Shifts.GetAll().Where(s => s.AssignedWorkerLogin != null).ToList();
        }
    }
}