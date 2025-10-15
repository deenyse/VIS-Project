using CV_3_project.Models;
using CV_3_project.Repositories;
using System.Text.Json;

namespace CV_3_project
{
    // Manages data persistence for JSON files as a single transaction.
    public class JsonUnitOfWork : IUnitOfWork
    {
        private List<Account> _accounts;
        private List<Shift> _shifts;
        private readonly string _accountsFilePath = "accounts.json";
        private readonly string _shiftsFilePath = "shifts.json";

        public IAccountRepository Accounts { get; }
        public IShiftRepository Shifts { get; }

        public JsonUnitOfWork()
        {
            _accounts = LoadData<Account>(_accountsFilePath);
            _shifts = LoadData<Shift>(_shiftsFilePath);

            // Pass in-memory lists to repositories
            Accounts = new JsonAccountRepository(_accounts);
            Shifts = new JsonShiftRepository(_shifts);
        }

        private List<T> LoadData<T>(string filePath)
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
            }
            return new List<T>();
        }

        // Commits all changes to the JSON files.
        public void SaveChanges()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };

            string accountsJson = JsonSerializer.Serialize(_accounts, options);
            File.WriteAllText(_accountsFilePath, accountsJson);

            string shiftsJson = JsonSerializer.Serialize(_shifts, options);
            File.WriteAllText(_shiftsFilePath, shiftsJson);
        }

        public void Dispose()
        {
            // Nothing to dispose for JSON files
        }
    }
}