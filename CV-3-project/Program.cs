using CV_3_project.Observers;
using CV_3_project.Services;
using EmbedIO;

namespace CV_3_project
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                IUnitOfWork unitOfWork = new MongoUnitOfWork();

                var shiftService = new ShiftService(unitOfWork);
                shiftService.EnsureRootAdminExists();

                var url = "http://localhost:9696/";

                var serverTask = Task.Run(() => StartWebServer(url, shiftService));

                Console.WriteLine("--------------------------------------------------");
                Console.WriteLine($"[WEB] Web Interface running at: {url}");
                Console.WriteLine("--------------------------------------------------");

                Application app = new Application(unitOfWork);
                app.RegisterObserver(new WorkerNotifier(unitOfWork));
                app.RegisterObserver(new ManagerNotifier(unitOfWork));

                Console.WriteLine("Server is running. Press enter to close");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Erorr: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Console.ResetColor();

                Console.WriteLine("\nPress Enter to close window");
                Console.ReadLine();
            }
        }

        private static void StartWebServer(string url, ShiftService service)
        {
            try
            {
                using (var server = new WebServer(o => o
                    .WithUrlPrefix(url)
                    .WithMode(HttpListenerMode.EmbedIO)))
                {
                    server
                        .WithWebApi("/api", m => m.RegisterController(() => new ShiftApiController(service)))
                        .WithStaticFolder("/", "html", true);

                    server.RunAsync().Wait();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Web Server Error]: {ex.Message}");
            }
        }
    }
}