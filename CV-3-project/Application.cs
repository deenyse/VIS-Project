using CV_3_project.Services;
using EmbedIO;

namespace CV_3_project
{
    class Application
    {
        private IUnitOfWork unitOfWork;
        private ShiftService shiftService;

        private string url = "http://localhost:9696/";
        public Application()
        {
            unitOfWork = new MongoUnitOfWork();
            shiftService = new ShiftService(unitOfWork);
        }

        public void Run()
        {
            try
            {
                Task.Run(() => StartWebServer());


                Console.WriteLine("--------------------------------------------------");
                Console.WriteLine($"[WEB] Web Interface running at: {url}");
                Console.WriteLine("--------------------------------------------------");

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

        private void StartWebServer()
        {
            try
            {
                using (var server = new WebServer(o => o
                    .WithUrlPrefix(url)
                    .WithMode(HttpListenerMode.EmbedIO)))
                {
                    server
                        .WithWebApi("/api", m => m.RegisterController(() => new ShiftApiController(shiftService)))
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