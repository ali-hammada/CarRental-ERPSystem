using Application.Providers;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;

namespace Web.Controllers
{
    [Authorize]
    public class TrackingController : Controller
    {
        private readonly ICarTrackingProvider _trackingProvider;
        private readonly ICarTrackingService _trackingService;
        private readonly IToastNotification _toast;

        public TrackingController(
            ICarTrackingProvider trackingProvider,
            ICarTrackingService trackingService,
            IToastNotification toast)
        {
            _trackingProvider = trackingProvider;
            _trackingService = trackingService;
            _toast = toast;
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
                _toast.AddErrorToastMessage("Vehicle record not found for tracking.");
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
            var result = await _trackingService.SimulateTelemetryPingAsync(id);
            if (result)
                _toast.AddSuccessToastMessage("GPS telemetry ping simulated successfully.");
            else
                _toast.AddErrorToastMessage("Failed to simulate GPS telemetry.");

            return RedirectToAction(nameof(Car), new { id = id });
        }
    }
}
