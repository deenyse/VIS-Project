namespace CV_3_project
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Create a single instance of the Unit of Work
            IUnitOfWork unitOfWork = new JsonUnitOfWork();

            // Pass it to the application
            Application app = new Application(unitOfWork);

            while (true)
            {
                Console.WriteLine("\n--- Shift Management System ---");
                Console.WriteLine("Select an option:");
                if (app.signedInUserLogin == null)
                {
                    Console.WriteLine("1. Login");
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
    }
}