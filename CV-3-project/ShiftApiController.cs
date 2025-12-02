using CV_3_project.Models;
using CV_3_project.Services;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;

namespace CV_3_project
{
    public class ShiftApiController : WebApiController
    {
        private readonly ShiftService _service;

        public ShiftApiController(ShiftService service)
        {
            _service = service;
        }

        [Route(HttpVerbs.Get, "/shifts")]
        public List<Shift> GetShifts() => _service.GetAvailableShifts();

        [Route(HttpVerbs.Post, "/shifts")]
        public async Task<bool> AddShift()
        {
            var data = await HttpContext.GetRequestFormDataAsync();
            if (DateTime.TryParse(data["start"], out var start) && DateTime.TryParse(data["end"], out var end))
            {
                return _service.AddShift(start, end) == null;
            }
            return false;
        }

        [Route(HttpVerbs.Post, "/assign")]
        public async Task<bool> AssignShift()
        {
            var data = await HttpContext.GetRequestFormDataAsync();
            if (int.TryParse(data["shiftId"], out var sId) && int.TryParse(data["workerId"], out var wId))
            {
                return _service.AssignToShift(sId, wId) == null;
            }
            return false;
        }
    }
}