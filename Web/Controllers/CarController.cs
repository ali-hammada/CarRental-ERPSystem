using Application.Providers;
using Application.Services;
using ApplicationCore.Entities;
using ApplicationCore.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;

namespace Web.Controllers
{
    [Authorize]
    public class CarController : Controller
    {
        private readonly ICarProvider _carProvider;
        private readonly ICarServices _carServices;
        private readonly IToastNotification _toastNotification;

        public CarController(ICarProvider carProvider, ICarServices carServices, IToastNotification toastNotification)
        {
            _carProvider = carProvider;
            _carServices = carServices;
            _toastNotification = toastNotification;
        }

        public async Task<IActionResult> Index()
        {
            var cars = await _carProvider.GetAllCarsAsync();
            return View(cars);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var car = new Car();
            return View(car);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Car car)
        {
            if (!ModelState.IsValid)
                return View(car);

            car.Status = CarStatus.Available;
            await _carServices.AddCarAsync(car);

            _toastNotification.AddSuccessToastMessage("Car added successfully!");
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var car = await _carServices.GetByIdAsync(id);
            if (car == null)
            {
                _toastNotification.AddErrorToastMessage("Car not found.");
                return RedirectToAction("Index");
            }
            return View(car);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Car car)
        {
            if (!ModelState.IsValid)
                return View(car);

            await _carServices.UpdateCarAsync(car);

            _toastNotification.AddSuccessToastMessage("Car updated successfully!");
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _carServices.DeleteCarAsync(id);
            if (!result)
            {
                _toastNotification.AddErrorToastMessage("Car could not be deleted.");
            }
            else
            {
                _toastNotification.AddSuccessToastMessage("Car deleted successfully.");
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id, CarStatus status)
        {
            var car = await _carServices.GetByIdAsync(id);
            if (car == null)
            {
                TempData["Error"] = "Car not found!";
                return RedirectToAction("Index");
            }

            car.Status = status;
            await _carServices.UpdateCarAsync(car);

            TempData["Success"] = "Car status updated!";
            return RedirectToAction("Index");
        }
    }
}
