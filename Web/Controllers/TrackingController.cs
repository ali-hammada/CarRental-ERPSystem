using Application.Providers;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [Authorize]
    public class TrackingController : Controller
    {
        private readonly ICarTrackingProvider _trackingProvider;
        private readonly ICarTrackingService _trackingService;

        public TrackingController(
            ICarTrackingProvider trackingProvider,
            ICarTrackingService trackingService)
        {
            _trackingProvider = trackingProvider;
            _trackingService = trackingService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var fleet = await _trackingProvider.GetAllFleetLocationsAsync();
            return View(fleet);
        }

        [HttpGet]
        public async Task<IActionResult> Car(int id)
        {
            var car = await _trackingProvider.GetCarCurrentLocationAsync(id);
            if (car == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var history = await _trackingProvider.GetCarLocationHistoryAsync(id);
            ViewBag.History = history;
            return View(car);
        }

        [HttpGet]
        public async Task<IActionResult> GetFleetJson()
        {
            var fleet = await _trackingProvider.GetAllFleetLocationsAsync();
            return Json(fleet);
        }

        [HttpGet]
        public async Task<IActionResult> GetCarHistoryJson(int id)
        {
            var history = await _trackingProvider.GetCarLocationHistoryAsync(id);
            return Json(history);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SimulatePing(int id)
        {
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            var result = await _trackingService.SimulateTelemetryPingAsync(id);
            if (isAjax) return Json(new { success = result, redirectUrl = Url.Action(nameof(Car), new { id = id }) });
            return RedirectToAction(nameof(Car), new { id = id });
        }
    }
}
