using CV_3_project.Repositories;
using MongoDB.Driver;

namespace CV_3_project
{
    public class MongoUnitOfWork : IUnitOfWork
    {
        private readonly IMongoDatabase _database;

        public IAccountRepository Accounts { get; }
        public IShiftRepository Shifts { get; }

        public MongoUnitOfWork()
        {
            const string connectionUri = "mongodb+srv://system:FRX56Ar8SnuQqa3q@shiftmanager.foyyhor.mongodb.net/?retryWrites=true&w=majority&appName=ShiftManager";

            var databaseName = "ShiftsDB";

            var client = new MongoClient(connectionUri);
            _database = client.GetDatabase(databaseName);

            Accounts = new MongoAccountRepository(_database);
            Shifts = new MongoShiftRepository(_database);
        }

        // With MongoDB, changes are saved as they happen.
        // This method is kept to satisfy the interface contract.
        public void SaveChanges()
        {
            // No explicit transaction commit needed for this basic setup.
        }

        public void Dispose()
        {
            // The MongoDB driver manages connections automatically.
            // No explicit disposal is necessary here.
        }
    }
}