using System.Text;
using System.Text.Json;

namespace ClientTerminal
{
    //DTO
    public class Shift
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public TimeRange Period { get; set; }
    }

    public class TimeRange
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public override string ToString()
        {
            return $"{StartTime} - {EndTime}";
        }
    }

    public class Notification
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Message { get; set; }
    }

    public class AssignedShiftDto
    {
        public Shift Shift { get; set; }
        public Worker Worker { get; set; }
    }

    public record ContactInfo(string Email, string Phone);

    public class Account
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
    }

    public class GuestAccount : Account { }
    public class AppManager : Account { }
    public class Manager : Account { }
    public class Worker : Account
    {
        public string Position { get; set; }
    }
    public class Application
    {
        private const string BaseUrl = "http://localhost:9696";

        private readonly HttpClient _client;
        public Account signedInUser { get; private set; } = new GuestAccount();

        private readonly JsonSerializerOptions _jsonOptions;
        public Application()
        {
            _client = new HttpClient();
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        //WORKS
        public bool Login(string login, string password)
        {
            try
            {
                var payload = new { Login = login, Password = password };

                string json = JsonSerializer.Serialize(payload);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = _client.PostAsync($"{BaseUrl}/api/login", content).Result;

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content.ReadAsStringAsync().Result;

                    if (string.IsNullOrWhiteSpace(responseContent) || responseContent == "null") return false;

                    using JsonDocument doc = JsonDocument.Parse(responseContent);
                    JsonElement root = doc.RootElement;

                    if (root.ValueKind != JsonValueKind.Object) return false;

                    string role = root.GetProperty("Role").GetString();
                    JsonElement userEl = root.GetProperty("User");


                    switch (role)
                    {
                        case "AppManager": signedInUser = JsonSerializer.Deserialize<AppManager>(userEl.GetRawText(), _jsonOptions); break;
                        case "Manager": signedInUser = JsonSerializer.Deserialize<Manager>(userEl.GetRawText(), _jsonOptions); break;
                        case "Worker": signedInUser = JsonSerializer.Deserialize<Worker>(userEl.GetRawText(), _jsonOptions); break;
                        default: return false;
                    }
                    return true;

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection error: {ex.Message}");
            }
            return false;
        }

        //WORKS
        public void Logout()
        {
            signedInUser = new GuestAccount();
        }

        public bool IsAppManager() => signedInUser is AppManager;
        public bool IsManager() => signedInUser is Manager;

        //WORKS
        public List<Shift> GetAvailableShifts()
        {
            try
            {
                var response = _client.GetAsync($"{BaseUrl}/api/shifts/available")
                                      .GetAwaiter()
                                      .GetResult();

                if (response.IsSuccessStatusCode)
                {
                    var json = response.Content.ReadAsStringAsync()
                                               .GetAwaiter()
                                               .GetResult();

                    if (string.IsNullOrWhiteSpace(json) || json == "null")
                        return new List<Shift>();


                    return JsonSerializer.Deserialize<List<Shift>>(json, _jsonOptions) ?? new List<Shift>();
                }
                else
                {
                    Console.WriteLine($"Server issue with status code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Issue during getting shifts: {ex.Message}");
            }

            return new List<Shift>();
        }

        public List<(Shift, Worker)> GetAssignedShiftsWithWorker()
        {
            try
            {
                var response = _client.GetAsync($"{BaseUrl}/api/shifts/assigned").Result;

                if (response.IsSuccessStatusCode)
                {
                    var json = response.Content.ReadAsStringAsync().Result;
                    if (string.IsNullOrWhiteSpace(json) || json == "null") return new List<(Shift, Worker)>();


                    var dtos = JsonSerializer.Deserialize<List<AssignedShiftDto>>(json, _jsonOptions);

                    return dtos?.Select(d => (d.Shift, d.Worker)).ToList() ?? new List<(Shift, Worker)>();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"GetAssignedShiftsWithWorker issue: {e}.");
            }
            return new List<(Shift, Worker)>();
        }

        //WORKS
        public bool AddShift(DateTime start, DateTime end)
        {
            try
            {
                var payload = new { WorkerId = signedInUser.Id, Start = start, End = end };

                string json = JsonSerializer.Serialize(payload);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = _client.PostAsync($"{BaseUrl}/api/shifts/create", content).Result;

                string boolStr = response.Content.ReadAsStringAsync().Result;
                return bool.TryParse(boolStr, out bool result) && result;
            }
            catch { return false; }
        }

        public bool AssignToShift(int shiftId)
        {
            try
            {
                var payload = new { WorkerId = signedInUser.Id, ShiftId = shiftId };

                string json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = _client.PostAsync($"{BaseUrl}/api/assign", content).Result;

                string boolStr = response.Content.ReadAsStringAsync().Result;
                return bool.TryParse(boolStr, out bool result) && result;
            }
            catch { return false; }
        }

        //WORKS
        public List<Notification> GetAndClearUserNotifications(int userId)
        {
            try
            {
                var response = _client.GetAsync($"{BaseUrl}/api/notifications/{userId}").Result;

                if (response.IsSuccessStatusCode)
                {
                    var json = response.Content.ReadAsStringAsync().Result;
                    if (string.IsNullOrWhiteSpace(json) || json == "null") return new List<Notification>();

                    return JsonSerializer.Deserialize<List<Notification>>(json, _jsonOptions) ?? new List<Notification>();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"GetAndClearUserNotifications issue: {e}.");
            }
            return new List<Notification>();
        }
        //ISSUE
        public void AddNotification(int targetWorkerId, string message)
        {
            try
            {
                var payload = new { WorkerId = targetWorkerId, UserId = signedInUser.Id, Message = message };

                string json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _client.PostAsync($"{BaseUrl}/api/notifications/send", content).Wait();
            }
            catch { Console.WriteLine("Ошибка отправки уведомления"); }
        }

        public bool AddManager(string login, string pass, string name, string sur, ContactInfo contact)
        {
            return CreateAccountRequest(login, pass, name, sur, contact, "manager", null);
        }

        public bool AddWorker(string login, string pass, string name, string sur, ContactInfo contact, string position)
        {
            return CreateAccountRequest(login, pass, name, sur, contact, "worker", position);
        }

        private bool CreateAccountRequest(string login, string pass, string name, string sur, ContactInfo contact, string type, string? position)
        {
            try
            {
                var payload = new
                {
                    WorkerId = signedInUser.Id,
                    Type = type,
                    Login = login,
                    Password = pass,
                    Name = name,
                    Surname = sur,
                    Email = contact.Email,
                    Phone = contact.Phone,
                    Position = position
                };

                string json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = _client.PostAsync($"{BaseUrl}/api/accounts/create", content).Result;
                var resultMsg = response.Content.ReadAsStringAsync().Result;

                return string.IsNullOrEmpty(resultMsg) || resultMsg == "null";
            }
            catch { return false; }
        }
    }

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
                        Console.WriteLine("3. Logout");
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
                            case "3": Logout(app); break;
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