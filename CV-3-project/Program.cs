using CV_3_project.Models;
using CV_3_project.Observers;

namespace CV_3_project
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IUnitOfWork unitOfWork = new MongoUnitOfWork();
            Application app = new Application(unitOfWork);

            app.RegisterObserver(new WorkerNotifier(unitOfWork));
            app.RegisterObserver(new ManagerNotifier(unitOfWork));

            while (true)
            {
                Console.WriteLine("\n--- Shift Management System ---");

                if (app.signedInUser is GuestAccount)
                {
                    Console.WriteLine("Logged in as: Guest");
                    Console.WriteLine("1. Login");
                    Console.WriteLine("2. Create Manager Account");
                    Console.WriteLine("3. Create Worker Account");
                }
                else
                {
                    Console.WriteLine($"Logged in as: {app.signedInUser.Name} {app.signedInUser.Surname} (ID: {app.signedInUser.Id})");
                    if (app.IsManager())
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
                        Console.WriteLine("3. Logout");
                    }
                }

                string choice = Console.ReadLine();

                if (app.signedInUser is GuestAccount)
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
                            SendNotification(app);
                            break;
                        case "4":
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
            {
                Console.WriteLine("✅ Login successful");
                CheckNotifications(app);
            }
            else
            {
                Console.WriteLine("Login failed");
            }
        }

        private static void CheckNotifications(Application app)
        {
            Console.WriteLine("\n--- Checking for notifications ---");
            var notifications = app.GetAndClearUserNotifications(app.signedInUser.Id);
            if (!notifications.Any())
            {
                Console.WriteLine("No Notifications.");
            }
            else
            {
                Console.WriteLine($"You have {notifications.Count} new notification(s):");
                foreach (var notification in notifications)
                {
                    Console.WriteLine($"[{notification.CreatedAt:g}] {notification.Message}");
                }
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
                Console.Write("Enter shift start time (yyyy-MM-dd HH:mm): ");
                DateTime startTime = DateTime.Parse(Console.ReadLine());
                Console.Write("Enter shift end time (yyyy-MM-dd HH:mm): ");
                DateTime endTime = DateTime.Parse(Console.ReadLine());

                if (app.AddShift(startTime, endTime))
                    Console.WriteLine("Shift added successfully");
                else
                    Console.WriteLine("Failed to add shift (check validation errors or invariant rules).");
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
                string workerName = $"{worker.Name} {worker.Surname}";
                Console.WriteLine($"Shift ID: {shift.Id}, Period: {shift.Period}, Worker: {workerName}");
            }
        }

        private static void ViewAvailableShifts(Application app)
        {
            var availableShifts = app.GetAvailableShifts();
            Console.WriteLine("--- Available Shifts ---");
            foreach (var shift in availableShifts)
            {
                Console.WriteLine($"Shift ID: {shift.Id}, Period: {shift.Period}");
            }
        }

        private static void AssignToShift(Application app)
        {
            Console.Write("Enter Shift ID to assign: ");
            int shiftId = int.Parse(Console.ReadLine());

            if (app.AssignToShift(shiftId))
                Console.WriteLine("Successfully assigned to shift");
            else
                Console.WriteLine("Failed to assign to shift (maybe it's taken or conflicts with your schedule).");
        }

        private static void SendNotification(Application app)
        {
            try
            {
                Console.Write("Enter Worker ID to send notification: ");
                int workerId = int.Parse(Console.ReadLine());
                Console.Write("Enter message: ");
                string message = Console.ReadLine();

                app.AddNotification(workerId, message);
                Console.WriteLine("Notification sent.");
            }
            catch (FormatException)
            {
                Console.WriteLine("Invalid Worker ID.");
            }
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

                var contacts = new ContactInfo(email, phone);
                bool success = false;

                if (isManager)
                {
                    success = app.AddManager(login, password, name, surname, contacts);
                }
                else
                {
                    Console.Write("Enter Working Position: ");
                    string position = Console.ReadLine();
                    success = app.AddWorker(login, password, name, surname, contacts, position);
                }

                if (success)
                    Console.WriteLine("✅ Account created successfully.");
                else
                    Console.WriteLine("⚠️ Failed to create account (check validation).");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error creating account: {ex.Message}");
            }
        }
    }
}