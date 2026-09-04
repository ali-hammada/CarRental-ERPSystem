using Application.Services;
using ApplicationCore.Entities;
using ApplicationCore.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [Authorize]
    public class MaintenanceController : Controller
    {
        private readonly IMaintenanceService _maintenanceService;
        private readonly ICarServices _carServices;

        public MaintenanceController(
            IMaintenanceService maintenanceService,
            ICarServices carServices)
        {
            _maintenanceService = maintenanceService;
            _carServices = carServices;
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
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            ModelState.Remove("Car");

            var allCars = await _carServices.GetAllCarsAsync();
            var targetCar = allCars.FirstOrDefault(c => c.Id == log.CarId);

            if (targetCar != null && targetCar.Status == CarStatus.Rented)
            {
                var errorMsg = $"Cannot record maintenance for vehicle '{targetCar.Model} ({targetCar.PlateNumber})' because it is currently RENTED out under an active contract. Please close the contract first.";
                if (isAjax) return Json(new { success = false, message = errorMsg });
                ViewBag.Cars = allCars;
                return View(log);
            }

            if (!ModelState.IsValid)
            {
                if (isAjax)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join("<br>", errors) });
                }
                ViewBag.Cars = allCars;
                return View(log);
            }

            await _maintenanceService.AddLogAsync(log);

            if (targetCar != null)
            {
                if (log.Status == "Completed")
                {
                    if (targetCar.Status == CarStatus.Maintenance)
                    {
                        targetCar.Status = CarStatus.Available;
                        await _carServices.UpdateCarAsync(targetCar);
                    }
                }
                else
                {
                    if (targetCar.Status == CarStatus.Available)
                    {
                        targetCar.Status = CarStatus.Maintenance;
                        await _carServices.UpdateCarAsync(targetCar);
                    }
                }
            }

            if (isAjax) return Json(new { success = true, redirectUrl = Url.Action(nameof(Index)) });
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            var log = await _maintenanceService.GetByIdAsync(id);
            if (log == null)
            {
                if (isAjax) return Json(new { success = false, message = "Maintenance record not found." });
                return RedirectToAction(nameof(Index));
            }

            log.Status = "Completed";
            await _maintenanceService.UpdateLogAsync(log);

            var car = await _carServices.GetByIdAsync(log.CarId);
            if (car != null && car.Status == CarStatus.Maintenance)
            {
                car.Status = CarStatus.Available;
                await _carServices.UpdateCarAsync(car);
            }

            if (isAjax) return Json(new { success = true, redirectUrl = Url.Action(nameof(Index)) });
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            await _maintenanceService.DeleteLogAsync(id);
            if (isAjax) return Json(new { success = true, redirectUrl = Url.Action(nameof(Index)) });
            return RedirectToAction(nameof(Index));
        }
    }
}
