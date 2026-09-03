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
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CarController(
            ICarProvider carProvider,
            ICarServices carServices,
            IToastNotification toastNotification,
            IWebHostEnvironment webHostEnvironment)
        {
            _carProvider = carProvider;
            _carServices = carServices;
            _toastNotification = toastNotification;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(string? type = "all")
        {
            var cars = await _carProvider.GetAllCarsAsync();
            // Exclude sold vehicles from active operational catalog
            cars = cars.Where(c => c.SaleStatus != CarSaleStatus.Sold && c.SaleStatus != CarSaleStatus.Reserved).ToList();

            if (type == "rental")
            {
                cars = cars.Where(c => c.ListingType == CarListingType.RentalOnly || c.ListingType == CarListingType.Both).ToList();
            }
            else if (type == "sale")
            {
                cars = cars.Where(c => c.ListingType == CarListingType.SaleOnly || c.ListingType == CarListingType.Both).ToList();
            }

            ViewBag.ActiveTab = type ?? "all";
            return View(cars);
        }

        [HttpGet]
        public async Task<IActionResult> SalesCatalog()
        {
            var cars = await _carProvider.GetAllCarsAsync();
            // Showroom catalog: Only display vehicles available for sale (exclude Sold/Reserved/OutOfService)
            var salesCars = cars.Where(c => (c.ListingType == CarListingType.SaleOnly || c.ListingType == CarListingType.Both)
                                         && c.SaleStatus != CarSaleStatus.Sold
                                         && c.SaleStatus != CarSaleStatus.Reserved
                                         && c.Status != CarStatus.OutOfService).ToList();
            return View(salesCars);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var car = new Car { ListingType = CarListingType.RentalOnly };
            return View(car);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Car car, IFormFile? ImageFile)
        {
            if (car.ListingType == CarListingType.SaleOnly)
            {
                ModelState.Remove("PricePerDay");
                car.PricePerDay = 0;
            }

            if (!ModelState.IsValid)
                return View(car);

            if (ImageFile != null && ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "cars");
                Directory.CreateDirectory(uploadsFolder);
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(fileStream);
                }

                car.ImageUrl = "/uploads/cars/" + uniqueFileName;
            }

            car.Status = CarStatus.Available;
            if (car.ListingType == CarListingType.SaleOnly || car.ListingType == CarListingType.Both)
            {
                car.SaleStatus = CarSaleStatus.ForSale;
                if (!car.SalePrice.HasValue && car.TargetSalePrice.HasValue)
                {
                    car.SalePrice = car.TargetSalePrice;
                }
            }

            await _carServices.AddCarAsync(car);

            _toastNotification.AddSuccessToastMessage("Car added successfully!");
            if (car.ListingType == CarListingType.SaleOnly)
                return RedirectToAction(nameof(SalesCatalog));

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var car = await _carServices.GetByIdAsync(id);
            if (car == null)
            {
                _toastNotification.AddErrorToastMessage("Car not found.");
                return RedirectToAction(nameof(Index));
            }
            return View(car);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Car car, IFormFile? ImageFile)
        {
            if (car.ListingType == CarListingType.SaleOnly)
            {
                ModelState.Remove("PricePerDay");
                car.PricePerDay = 0;
            }

            if (!ModelState.IsValid)
                return View(car);

            if (ImageFile != null && ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "cars");
                Directory.CreateDirectory(uploadsFolder);
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(fileStream);
                }

                car.ImageUrl = "/uploads/cars/" + uniqueFileName;
            }

            await _carServices.UpdateCarAsync(car);

            _toastNotification.AddSuccessToastMessage("Car updated successfully!");
            return RedirectToAction(nameof(Index));
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadContractDocument(int carId, IFormFile? OriginalPurchaseContract, IFormFile? FinalBuyerContract)
        {
            var car = await _carServices.GetByIdAsync(carId);
            if (car == null)
            {
                _toastNotification.AddErrorToastMessage("Vehicle record not found.");
                return RedirectToAction(nameof(SalesCatalog));
            }

            var contractsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "contracts");
            Directory.CreateDirectory(contractsFolder);

            if (OriginalPurchaseContract != null && OriginalPurchaseContract.Length > 0)
            {
                var fileName = $"PurchaseContract_Car{car.Id}_{Guid.NewGuid().ToString().Substring(0, 8)}{Path.GetExtension(OriginalPurchaseContract.FileName)}";
                var filePath = Path.Combine(contractsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await OriginalPurchaseContract.CopyToAsync(stream);
                }
                car.OriginalPurchaseContractUrl = "/uploads/contracts/" + fileName;
            }

            if (FinalBuyerContract != null && FinalBuyerContract.Length > 0)
            {
                var fileName = $"BuyerContract_Car{car.Id}_{Guid.NewGuid().ToString().Substring(0, 8)}{Path.GetExtension(FinalBuyerContract.FileName)}";
                var filePath = Path.Combine(contractsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await FinalBuyerContract.CopyToAsync(stream);
                }
                car.FinalBuyerContractUrl = "/uploads/contracts/" + fileName;
            }

            await _carServices.UpdateCarAsync(car);
            _toastNotification.AddSuccessToastMessage("Legal & Financial contract documents saved successfully!");
            return RedirectToAction(nameof(SalesCatalog));
        }

        [HttpGet]
        public async Task<IActionResult> Alerts()
        {
            var cars = await _carProvider.GetAllCarsAsync();
            var now = DateTime.UtcNow;

            ViewBag.LicenseExpiring = cars.Where(c => c.LicenseExpiryDate.HasValue && c.LicenseExpiryDate.Value <= now.AddDays(30)).OrderBy(c => c.LicenseExpiryDate).ToList();
            ViewBag.InsuranceExpiring = cars.Where(c => c.InsuranceExpiryDate.HasValue && c.InsuranceExpiryDate.Value <= now.AddDays(30)).OrderBy(c => c.InsuranceExpiryDate).ToList();
            ViewBag.MaintenanceDue = cars.Where(c => c.CurrentOdometer >= 50000 || c.Status == CarStatus.Maintenance).ToList();

            return View(cars);
        }
    }
}
