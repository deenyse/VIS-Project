﻿using CV_3_project.Models;

namespace CV_3_project
{
    class Application
    {
        private readonly IUnitOfWork _unitOfWork;
        public Account? signedInUser = null; // Теперь храним весь объект пользователя

        public Application(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public void AddManager(string login, string password, string name, string surname, ContactInfo contacts)
        {
            _unitOfWork.Accounts.Add(new Manager(login, password, name, surname, contacts));
            _unitOfWork.SaveChanges();
        }

        public void AddWorker(string login, string password, string name, string surname, ContactInfo contacts)
        {
            _unitOfWork.Accounts.Add(new Worker(login, password, name, surname, contacts));
            _unitOfWork.SaveChanges();
        }

        public bool AddShift(DateTime startTime, DateTime endTime)
        {
            if (!IsManager())
                return false;

            _unitOfWork.Shifts.Add(new Shift(startTime, endTime));
            _unitOfWork.SaveChanges();
            return true;
        }

        public bool AssignToShift(string shiftId)
        {
            if (signedInUser == null) return false;

            var shift = _unitOfWork.Shifts.GetById(shiftId);
            if (shift == null || shift.AssignedWorkerId != null)
                return false;

            bool hasConflict = _unitOfWork.Shifts.GetAll().Any(s =>
                s.AssignedWorkerId == signedInUser.MongoId &&
                s.StartTime < shift.EndTime &&
                s.EndTime > shift.StartTime
            );

            if (hasConflict)
                return false;

            shift.AssignedWorkerId = signedInUser.MongoId;
            _unitOfWork.Shifts.Update(shift);
            _unitOfWork.SaveChanges();
            return true;
        }

        public bool Login(string login, string password)
        {
            var account = _unitOfWork.Accounts.GetByLogin(login);
            if (account != null && account.Password == password) // Проверка пароля (в проде - хеша)
            {
                signedInUser = account;
                return true;
            }
            return false;
        }

        public void Logout()
        {
            signedInUser = null;
        }

        public bool IsManager()
        {
            return signedInUser is Manager;
        }

        public List<Shift> GetAvailableShifts()
        {
            return _unitOfWork.Shifts.GetAll().Where(s => s.AssignedWorkerId == null).ToList();
        }

        // Возвращаем кортеж для удобного отображения информации
        public List<(Shift shift, Account? worker)> GetAssignedShiftsWithWorker()
        {
            var assignedShifts = _unitOfWork.Shifts.GetAll().Where(s => s.AssignedWorkerId != null).ToList();
            var workers = _unitOfWork.Accounts.GetAll()
                           .ToDictionary(a => a.MongoId, a => a);

            return assignedShifts.Select(s => (s, workers.GetValueOrDefault(s.AssignedWorkerId!))).ToList();
        }
    }
}