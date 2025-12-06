using System.Text;
using System.Text.Json;

namespace ClientTerminal
{
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


        public void Logout()
        {
            signedInUser = new GuestAccount();
        }

        public bool IsAppManager() => signedInUser is AppManager;
        public bool IsManager() => signedInUser is Manager;


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
                Console.WriteLine($"Issue during getting available shifts: {ex.Message}");
            }

            return new List<Shift>();
        }

        public List<Shift> GetWorkerShifts()
        {
            try
            {
                var response = _client.GetAsync($"{BaseUrl}/api/shifts/worker/{signedInUser.Id}")
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
                Console.WriteLine($"Issue during getting worker shifts: {ex.Message}");
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

        public void AddNotification(int targetWorkerId, string message)
        {
            try
            {
                var payload = new
                {

                    WorkerId = signedInUser.Id,
                    UserId = targetWorkerId,
                    Message = message
                };

                string json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = _client.PostAsync($"{BaseUrl}/api/notifications/send", content).Result;

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Notification sent successfully.");
                }
                else
                {
                    string serverError = response.Content.ReadAsStringAsync().Result;
                    Console.WriteLine($"Error during notification sending (Code {response.StatusCode}): {serverError}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AddNotification Error: {ex.Message}");
            }
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
}
