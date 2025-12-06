namespace ClientTerminal
{
    internal class Program
    {
        static Application app = new Application();

        static void Main(string[] args)
        {
            while (true)
            {
                string choice = "0";
                Console.WriteLine("\n--- Shift Management System (Console Client) ---");

                if (app.signedInUser is GuestAccount)
                {


                    Console.WriteLine("Logged in as: Guest! Please provide credentials to sign in.");
                    Login(app);
                }
                else
                {
                    Console.WriteLine($"Logged in as: {app.signedInUser.Name} {app.signedInUser.Surname} (ID: {app.signedInUser.Id})");
                    if (app.IsAppManager())
                    {
                        Console.WriteLine("1. Create Manager Account");
                        Console.WriteLine("2. Create Worker Account");
                        Console.WriteLine("3. Logout");
                    }
                    else if (app.IsManager())
                    {
                        Console.WriteLine("1. Add Shift");
                        Console.WriteLine("2. View Assigned Shifts");
                        Console.WriteLine("3. Send Notification to Worker");
                        Console.WriteLine("4. Logout");
                    }
                    else // Worker
                    {
                        Console.WriteLine("1. View Available Shifts");
                        Console.WriteLine("2. Assign to Shift");
                        Console.WriteLine("3. View my shifts");
                        Console.WriteLine("4. Logout");
                    }
                    Console.Write("Action: ");
                    choice = Console.ReadLine();

                    if (app.IsAppManager())
                    {
                        switch (choice)
                        {
                            case "1": CreateAccount(app, isManager: true); break;
                            case "2": CreateAccount(app, isManager: false); break;
                            case "3": Logout(app); break;
                        }
                    }
                    else if (app.IsManager())
                    {
                        switch (choice)
                        {
                            case "1": AddShift(app); break;
                            case "2": ViewAssignedShifts(app); break;
                            case "3": SendNotification(app); break;
                            case "4": Logout(app); break;
                        }
                    }
                    else if (app.signedInUser is Worker)
                    {
                        switch (choice)
                        {
                            case "1": ViewAvailableShifts(app); break;
                            case "2": AssignToShift(app); break;
                            case "3": ViewMyShifts(app); break;
                            case "4": Logout(app); break;
                        }
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
            {
                Console.WriteLine("✅ Login successful");
                CheckNotifications(app);
            }
            else Console.WriteLine("❌ Login failed (Wrong credentials or server error)");
        }

        private static void CheckNotifications(Application app)
        {
            Console.WriteLine("\n--- Checking for notifications ---");
            var notifications = app.GetAndClearUserNotifications(app.signedInUser.Id);
            if (!notifications.Any()) Console.WriteLine("No Notifications.");
            else
            {
                Console.WriteLine($"You have {notifications.Count} new notification(s):");
                foreach (var n in notifications) Console.WriteLine($"[{n.CreatedAt:g}] {n.Message}");
            }
            Console.WriteLine("--- End of notifications ---\n");
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
                Console.Write("Enter start (yyyy-MM-dd HH:mm): ");
                DateTime startTime = DateTime.Parse(Console.ReadLine());
                Console.Write("Enter end (yyyy-MM-dd HH:mm): ");
                DateTime endTime = DateTime.Parse(Console.ReadLine());

                if (app.AddShift(startTime, endTime)) Console.WriteLine("✅ Shift added.");
                else Console.WriteLine("❌ Failed to add shift (Server error).");
            }
            catch { Console.WriteLine("Invalid date format."); }
        }

        private static void ViewAssignedShifts(Application app)
        {
            var assignedShifts = app.GetAssignedShiftsWithWorker();
            Console.WriteLine("--- Assigned Shifts ---");
            if (assignedShifts.Count == 0) Console.WriteLine("No assigned shifts found.");

            foreach (var (shift, worker) in assignedShifts)
            {
                Console.WriteLine($"ID: {shift.Id}, Period: {shift.Period}, Worker: {worker.Name} {worker.Surname}");
            }
        }

        private static void ViewAvailableShifts(Application app)
        {
            var availableShifts = app.GetAvailableShifts();
            Console.WriteLine("--- Available Shifts ---");
            if (availableShifts.Count == 0) Console.WriteLine("No available shifts.");

            foreach (var shift in availableShifts)
            {
                Console.WriteLine($"ID: {shift.Id}, Period: {shift.Period}");
            }
        }

        private static void ViewMyShifts(Application app)
        {
            var workerShifts = app.GetWorkerShifts();
            Console.WriteLine("--- Available Shifts ---");
            if (workerShifts.Count == 0) Console.WriteLine("No available shifts.");

            foreach (var shift in workerShifts)
            {
                Console.WriteLine($"ID: {shift.Id}, Period: {shift.Period}");
            }
        }

        private static void AssignToShift(Application app)
        {
            Console.Write("Enter Shift ID: ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                if (app.AssignToShift(id)) Console.WriteLine("✅ Assigned.");
                else Console.WriteLine("❌ Failed (conflict, taken, or server error).");
            }
            else Console.WriteLine("Invalid ID.");
        }

        private static void SendNotification(Application app)
        {
            Console.Write("Worker ID: ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                Console.Write("Message: ");
                string msg = Console.ReadLine();
                app.AddNotification(id, msg);
                Console.WriteLine("Sent.");
            }
            else Console.WriteLine("Invalid ID.");
        }

        private static void CreateAccount(Application app, bool isManager)
        {
            try
            {
                Console.Write("Login: "); string login = Console.ReadLine();
                Console.Write("Password: "); string pass = Console.ReadLine();
                Console.Write("Name: "); string name = Console.ReadLine();
                Console.Write("Surname: "); string sur = Console.ReadLine();
                Console.Write("Email: "); string email = Console.ReadLine();
                Console.Write("Phone: "); string phone = Console.ReadLine();
                var contact = new ContactInfo(email, phone);

                bool success;
                if (isManager) success = app.AddManager(login, pass, name, sur, contact);
                else
                {
                    Console.Write("Position: "); string pos = Console.ReadLine();
                    success = app.AddWorker(login, pass, name, sur, contact, pos);
                }

                if (success) Console.WriteLine("✅ Account created.");
                else Console.WriteLine("⚠️ Failed (Login exists or server error).");
            }
            catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }
        }
    }
}