using CV_3_project.Models;
using CV_3_project.Services;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using System.Text.Json;

namespace CV_3_project
{
    public class LoginRequest
    {
        public string Login { get; set; }
        public string Password { get; set; }
    }

    public class AssignRequest
    {
        public int WorkerId { get; set; }
        public int ShiftId { get; set; }

    }

    public class CreateRequest
    {
        public int WorkerId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
    public class NotificationRequest
    {
        public int WorkerId { get; set; }
        public int UserId { get; set; }
        public string Message { get; set; }
    }
    public class CreateAccountRequest
    {
        public int WorkerId { get; set; }
        public string Type { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string? Position { get; set; }
    }
    public class ShiftApiController : WebApiController
    {
        private readonly ShiftService _service;

        public ShiftApiController(ShiftService service)
        {
            _service = service;
        }

        [Route(HttpVerbs.Post, "/login")]
        public async Task<object?> Login()
        {
            string body;
            using (var reader = new StreamReader(HttpContext.OpenRequestStream()))
            {
                body = await reader.ReadToEndAsync();
            }

            //Console.WriteLine($"[DEBUG] Login JSON: {body}");

            if (string.IsNullOrWhiteSpace(body)) return null;

            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var requestData = JsonSerializer.Deserialize<LoginRequest>(body, options);

                if (requestData == null || string.IsNullOrEmpty(requestData.Login)) return null;

                var account = _service.Login(requestData.Login, requestData.Password);

                if (account != null)
                {
                    return new
                    {
                        User = account,
                        Role = account.GetType().Name
                    };
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"[ERROR] Login Parsing failed: {ex.Message}");
            }
            return null;
        }

        [Route(HttpVerbs.Get, "/shifts/available")]
        public List<Shift> GetShifts() => _service.GetAvailableShifts();

        [Route(HttpVerbs.Get, "/shifts/assigned")]
        public List<AssignedShiftDto> GetAssignedShifts()
        {
            var shifts = _service.GetAssignedShifts();

            return _service.GetAssignedShiftsWithWorkers();
        }

        [Route(HttpVerbs.Get, "/shifts/worker/{workerId}")]
        public List<Shift> GetWorkerShifts(int workerId)
        {
            return _service.GetAssignedShifts()
                           .Where(s => s.AssignedWorkerId == workerId)
                           .ToList();
        }

        [Route(HttpVerbs.Post, "/shifts/create")]
        public async Task<bool> AddShift()
        {
            string body;
            using (var reader = new StreamReader(HttpContext.OpenRequestStream()))
            {
                body = await reader.ReadToEndAsync();
            }

            if (string.IsNullOrWhiteSpace(body)) return false;

            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var request = JsonSerializer.Deserialize<CreateRequest>(body, options);

                if (request == null)
                {
                    //Console.WriteLine("[ERROR] Create JSON invalid");
                    return false;
                }

                string? error = _service.AddShift(request.WorkerId, request.Start, request.End);
                if (error != null)
                {
                    //Console.WriteLine($"[ERROR] Create failed: {error}");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"[ERROR] Create Exception: {ex.Message}");
                return false;
            }
        }

        [Route(HttpVerbs.Post, "/assign")]
        public async Task<bool> AssignShift()
        {
            string body;
            using (var reader = new StreamReader(HttpContext.OpenRequestStream()))
            {
                body = await reader.ReadToEndAsync();
            }

            if (string.IsNullOrWhiteSpace(body)) return false;

            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var request = JsonSerializer.Deserialize<AssignRequest>(body, options);

                if (request == null || request.ShiftId == 0 || request.WorkerId == 0)
                {
                    Console.WriteLine("[ERROR] Assign JSON invalid");
                    return false;
                }

                string? error = _service.AssignToShift(request.WorkerId, request.ShiftId);
                if (error != null)
                {
                    Console.WriteLine($"[ERROR] Assign failed: {error}");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Assign Exception: {ex.Message}");
                return false;
            }
        }

        [Route(HttpVerbs.Get, "/notifications/{userId}")]
        public List<Notification> GetNotifications(int userId)
        {
            return _service.GetAndClearNotifications(userId);
        }

        [Route(HttpVerbs.Post, "/notifications/send")]
        public async Task<bool> SendNotification()
        {
            string body;
            using (var reader = new StreamReader(HttpContext.OpenRequestStream()))
            {
                body = await reader.ReadToEndAsync();
            }

            if (string.IsNullOrWhiteSpace(body)) return false;

            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var request = JsonSerializer.Deserialize<NotificationRequest>(body, options);

                if (request == null || request.UserId == 0)
                {
                    Console.WriteLine("[ERROR] Notification JSON invalid");
                    return false;
                }

                _service.AddNotification(request.WorkerId, request.UserId, request.Message);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Notification Exception: {ex.Message}");
                return false;
            }
        }

        [Route(HttpVerbs.Post, "/accounts/create")]
        public async Task<string?> CreateAccount()
        {
            string body;
            using (var reader = new StreamReader(HttpContext.OpenRequestStream()))
            {
                body = await reader.ReadToEndAsync();
            }

            if (string.IsNullOrWhiteSpace(body)) return "Empty request body";

            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var data = JsonSerializer.Deserialize<CreateAccountRequest>(body, options);

                if (data == null) return "Invalid JSON format";

                string login = data.Login ?? "";
                string pass = data.Password ?? "";
                string name = data.Name ?? "";
                string sur = data.Surname ?? "";
                string mail = data.Email ?? "";
                string ph = data.Phone ?? "";
                string pos = data.Position ?? "";

                if (data.Type == "manager")
                {
                    return _service.CreateManager(data.WorkerId, login, pass, name, sur, mail, ph);
                }
                else if (data.Type == "worker")
                {
                    return _service.CreateWorker(data.WorkerId, login, pass, name, sur, mail, ph, pos);
                }

                return "Invalid account type";
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"[ERROR] Create Account Exception: {ex.Message}");
                return $"Server Error: {ex.Message}";
            }
        }
    }
}