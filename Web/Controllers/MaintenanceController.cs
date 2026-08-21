using Application.Services;
using ApplicationCore.Entities;
using ApplicationCore.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;

namespace Web.Controllers
{
    [Authorize]
    public class MaintenanceController : Controller
    {
        private readonly IMaintenanceService _maintenanceService;
        private readonly ICarServices _carServices;
        private readonly IToastNotification _toast;

        public MaintenanceController(
            IMaintenanceService maintenanceService,
            ICarServices carServices,
            IToastNotification toast)
        {
            _maintenanceService = maintenanceService;
            _carServices = carServices;
            _toast = toast;
        }

        public async Task<IActionResult> Index()
        {
            var logs = await _maintenanceService.GetAllLogsAsync();
            return View(logs);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? carId)
        {
            var allCars = await _carServices.GetAllCarsAsync();
            ViewBag.Cars = allCars;

            if (carId.HasValue && carId > 0)
            {
                var selectedCar = allCars.FirstOrDefault(c => c.Id == carId.Value);
                if (selectedCar != null && selectedCar.Status == CarStatus.Rented)
                {
                    _toast.AddWarningToastMessage($"Vehicle '{selectedCar.Model} ({selectedCar.PlateNumber})' is currently RENTED under an active contract and cannot undergo maintenance until returned.");
                }
            }

            var model = new MaintenanceLog
            {
                CarId = carId ?? 0,
                ServiceDate = DateTime.Today
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MaintenanceLog log)
        {
            ModelState.Remove("Car");

            var allCars = await _carServices.GetAllCarsAsync();
            var targetCar = allCars.FirstOrDefault(c => c.Id == log.CarId);

            if (targetCar != null && targetCar.Status == CarStatus.Rented)
            {
                _toast.AddErrorToastMessage($"Cannot record maintenance for vehicle '{targetCar.Model} ({targetCar.PlateNumber})' because it is currently RENTED out under an active contract. Please close the contract first.");
                ViewBag.Cars = allCars;
                return View(log);
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Cars = allCars;
                return View(log);
            }

            await _maintenanceService.AddLogAsync(log);

            // Update vehicle status to Maintenance if it was Available
            if (targetCar != null && targetCar.Status == CarStatus.Available)
            {
                targetCar.Status = CarStatus.Maintenance;
                await _carServices.UpdateCarAsync(targetCar);
            }

            _toast.AddSuccessToastMessage("Maintenance log recorded successfully & vehicle status set to Maintenance.");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _maintenanceService.DeleteLogAsync(id);
            _toast.AddSuccessToastMessage("Maintenance log removed.");
            return RedirectToAction(nameof(Index));
        }
    }
}
