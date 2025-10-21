namespace CV_3_project
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Create a single instance of the Unit of Work using the new MongoDB implementation
            IUnitOfWork unitOfWork = new MongoUnitOfWork();

            // Pass it to the application
            Application app = new Application(unitOfWork);

            while (true)
            {
                Console.WriteLine("\n--- Shift Management System ---");
                Console.WriteLine("Select an option:");
                if (app.signedInUserLogin == null)
                {
                    Console.WriteLine("1. Login");
                    Console.WriteLine("2. Create Manager Account");
                    Console.WriteLine("3. Create Worker Account");
                }
                else if (app.IsManager())
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

                string choice = Console.ReadLine();

                if (app.signedInUserLogin == null)
                {
                    if (choice == "1")
                    {
                        Console.Write("Enter login: ");
                        string login = Console.ReadLine();
                        Console.Write("Enter password: ");
                        string password = Console.ReadLine();

                        if (app.Login(login, password))
                            Console.WriteLine("Login successful");
                        else
                            Console.WriteLine("Login failed");
                    }
                    else if (choice == "2")
                    {
                        // Create Manager Account
                        CreateAccount(app, isManager: true);
                    }
                    else if (choice == "3")
                    {
                        // Create Worker Account
                        CreateAccount(app, isManager: false);
                    }
                }
                else if (app.IsManager())
                {
                    if (choice == "1")
                    {
                        Console.Write("Enter shift start time (yyyy-MM-dd HH:mm): ");
                        if (DateTime.TryParse(Console.ReadLine(), out DateTime startTime))
                        {
                            Console.Write("Enter shift end time (yyyy-MM-dd HH:mm): ");
                            if (DateTime.TryParse(Console.ReadLine(), out DateTime endTime))
                            {
                                if (app.AddShift(startTime, endTime))
                                    Console.WriteLine("Shift added successfully");
                                else
                                    Console.WriteLine("Failed to add shift");
                            }
                            else { Console.WriteLine("Invalid date format."); }
                        }
                        else { Console.WriteLine("Invalid date format."); }
                    }
                    else if (choice == "2")
                    {
                        var assignedShifts = app.GetAssignedShifts();
                        Console.WriteLine("--- Assigned Shifts ---");
                        foreach (var shift in assignedShifts)
                        {
                            Console.WriteLine($"Shift ID: {shift.Id}, Start: {shift.StartTime}, End: {shift.EndTime}, Worker: {shift.AssignedWorkerLogin}");
                        }
                    }
                    else if (choice == "3")
                    {
                        app.Logout();
                        Console.WriteLine("Logged out");
                    }
                }
                else // Worker
                {
                    if (choice == "1")
                    {
                        var availableShifts = app.GetAvailableShifts();
                        Console.WriteLine("--- Available Shifts ---");
                        foreach (var shift in availableShifts)
                        {
                            Console.WriteLine($"Shift ID: {shift.Id}, Start: {shift.StartTime}, End: {shift.EndTime}");
                        }
                    }
                    else if (choice == "2")
                    {
                        Console.Write("Enter Shift ID to assign: ");
                        if (int.TryParse(Console.ReadLine(), out int shiftId))
                        {
                            if (app.AssignToShift(shiftId))
                                Console.WriteLine("Successfully assigned to shift");
                            else
                                Console.WriteLine("Failed to assign to shift (maybe it's taken or conflicts with your schedule).");
                        }
                        else
                        {
                            Console.WriteLine("Invalid ID.");
                        }
                    }
                    else if (choice == "3")
                    {
                        app.Logout();
                        Console.WriteLine("Logged out");
                    }
                }
            }
        }

        /// <summary>
        /// Handles the logic for creating a new user account.
        /// </summary>
        /// <param name="app">The application instance.</param>
        /// <param name="isManager">True to create a Manager, false to create a Worker.</param>
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

                if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(surname))
                {
                    Console.WriteLine("All fields are required. Account creation failed.");
                    return;
                }

                if (isManager)
                {
                    app.AddManager(login, password, name, surname);
                    Console.WriteLine("✅ Manager account created successfully.");
                }
                else
                {
                    app.AddWorker(login, password, name, surname);
                    Console.WriteLine("✅ Worker account created successfully.");
                }
            }
            catch (Exception ex)
            {
                // This will catch the exception if the login already exists
                Console.WriteLine($"⚠️ Error creating account: {ex.Message}");
            }
        }
    }
}