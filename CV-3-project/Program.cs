using CV_3_project.Models;

namespace CV_3_project
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IUnitOfWork unitOfWork = new MongoUnitOfWork();
            Application app = new Application(unitOfWork);

            while (true)
            {
                Console.WriteLine("\n--- Shift Management System ---");
                Console.WriteLine("Select an option:");
                if (app.signedInUser == null)
                {
                    Console.WriteLine("1. Login");
                    Console.WriteLine("2. Create Manager Account");
                    Console.WriteLine("3. Create Worker Account");
                }
                else
                {
                    Console.WriteLine($"Logged in as: {app.signedInUser.Name} {app.signedInUser.Surname}");
                    if (app.IsManager())
                    {
                        Console.WriteLine("1. Add Shift");
                        Console.WriteLine("2. View Assigned Shifts");
                        Console.WriteLine("3. Logout");
                    }
                    else // Worker
                    {
                        Console.WriteLine("1. View Available Shifts");
                        Console.WriteLine("2. Assign to Shift");
                        Console.WriteLine("3. Logout");
                    }
                }

                string choice = Console.ReadLine();

                if (app.signedInUser == null)
                {
                    switch (choice)
                    {
                        case "1":
                            Login(app);
                            break;
                        case "2":
                            CreateAccount(app, isManager: true);
                            break;
                        case "3":
                            CreateAccount(app, isManager: false);
                            break;
                    }
                }
                else if (app.IsManager())
                {
                    switch (choice)
                    {
                        case "1":
                            AddShift(app);
                            break;
                        case "2":
                            ViewAssignedShifts(app);
                            break;
                        case "3":
                            Logout(app);
                            break;
                    }
                }
                else // Worker
                {
                    switch (choice)
                    {
                        case "1":
                            ViewAvailableShifts(app);
                            break;
                        case "2":
                            AssignToShift(app);
                            break;
                        case "3":
                            Logout(app);
                            break;
                    }
                }
            }
        }

        private static void Login(Application app)
        {
            Console.Write("Enter login: ");
            string login = Console.ReadLine();
            Console.Write("Enter password: ");
            string password = Console.ReadLine();

            if (app.Login(login, password))
                Console.WriteLine("✅ Login successful");
            else
                Console.WriteLine("Login failed");
        }

        private static void Logout(Application app)
        {
            app.Logout();
            Console.WriteLine("Logged out");
        }

        private static void AddShift(Application app)
        {
            try
            {
                Console.Write("Enter shift start time (yyyy-MM-dd HH:mm): ");
                DateTime startTime = DateTime.Parse(Console.ReadLine());
                Console.Write("Enter shift end time (yyyy-MM-dd HH:mm): ");
                DateTime endTime = DateTime.Parse(Console.ReadLine());

                if (app.AddShift(startTime, endTime))
                    Console.WriteLine("Shift added successfully");
                else
                    Console.WriteLine("Failed to add shift");
            }
            catch (FormatException)
            {
                Console.WriteLine("Invalid date format.");
            }
        }

        private static void ViewAssignedShifts(Application app)
        {
            var assignedShifts = app.GetAssignedShiftsWithWorker();
            Console.WriteLine("--- Assigned Shifts ---");
            foreach (var (shift, worker) in assignedShifts)
            {
                string workerName = worker != null ? $"{worker.Name} {worker.Surname}" : "Unknown Worker";
                Console.WriteLine($"Shift ID: {shift.MongoId}, Start: {shift.StartTime}, End: {shift.EndTime}, Worker: {workerName}");
            }
        }

        private static void ViewAvailableShifts(Application app)
        {
            var availableShifts = app.GetAvailableShifts();
            Console.WriteLine("--- Available Shifts ---");
            foreach (var shift in availableShifts)
            {
                Console.WriteLine($"Shift ID: {shift.MongoId}, Start: {shift.StartTime}, End: {shift.EndTime}");
            }
        }

        private static void AssignToShift(Application app)
        {
            Console.Write("Enter Shift ID to assign: ");
            string shiftId = Console.ReadLine();

            if (app.AssignToShift(shiftId))
                Console.WriteLine("Successfully assigned to shift");
            else
                Console.WriteLine("Failed to assign to shift (maybe it's taken or conflicts with your schedule).");
        }

        private static void CreateAccount(Application app, bool isManager)
        {
            try
            {
                Console.Write("Enter new login: ");
                string login = Console.ReadLine();
                Console.Write("Enter new password: ");
                string password = Console.ReadLine();
                Console.Write("Enter name: ");
                string name = Console.ReadLine();
                Console.Write("Enter surname: ");
                string surname = Console.ReadLine();
                Console.Write("Enter email: ");
                string email = Console.ReadLine();
                Console.Write("Enter phone number: ");
                string phone = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(surname))
                {
                    Console.WriteLine("Login, password, name, and surname are required. Account creation failed.");
                    return;
                }

                var contacts = new ContactInfo(email, phone);

                if (isManager)
                {
                    app.AddManager(login, password, name, surname, contacts);
                    Console.WriteLine("✅ Manager account created successfully.");
                }
                else
                {
                    app.AddWorker(login, password, name, surname, contacts);
                    Console.WriteLine("✅ Worker account created successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error creating account: {ex.Message}");
            }
        }
    }
}