using Application.DTOs;
using Application.Providers;
using Application.Services;
using Application.Services.DTOs;
using ApplicationCore.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;
using Web.ViewModels;

namespace Web.Controllers
{
    [Authorize]
    public class RentalsController : Controller
    {
        private readonly IRentalProvider _rentalProvider;
        private readonly ICarProvider _carProvider;
        private readonly ICustomerProvider _customerProvider;
        private readonly IRentalServices _rentalServices;
        private readonly IPaymentServices _paymentServices;
        private readonly IAuditServices _auditServices;
        private readonly IToastNotification _toast;

        public RentalsController(
            IRentalProvider rentalProvider,
            ICarProvider carProvider,
            ICustomerProvider customerProvider,
            IRentalServices rentalServices,
            IPaymentServices paymentServices,
            IAuditServices auditServices,
            IToastNotification toast)
        {
            _rentalProvider = rentalProvider;
            _carProvider = carProvider;
            _customerProvider = customerProvider;
            _rentalServices = rentalServices;
            _paymentServices = paymentServices;
            _auditServices = auditServices;
            _toast = toast;
        }

        private int GetCurrentEmployeeId()
        {
            var employeeIdClaim = User.FindFirst("EmployeeId")?.Value
                               ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(employeeIdClaim))
                throw new UnauthorizedAccessException("User is not authenticated.");
            return int.Parse(employeeIdClaim);
        }

        private bool IsUserAdmin()
        {
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var name = User.Identity?.Name;
            return User.IsInRole("Admin") || User.IsInRole("Administrator") ||
                   string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase) ||
                   (!string.IsNullOrEmpty(name) && name.Contains("admin", StringComparison.OrdinalIgnoreCase));
        }

        public async Task<IActionResult> Index()
        {
            List<RentalListDto> rentals;
            if (IsUserAdmin())
            {
                rentals = await _rentalProvider.GetAllRentalsAsync();
            }
            else
            {
                int employeeId = GetCurrentEmployeeId();
                rentals = await _rentalProvider.GetEmployeeRentalsAsync(employeeId);
            }
            return View(rentals);
        }

        [HttpGet]
        public async Task<IActionResult> Open(int? carId)
        {
            if (carId.HasValue)
            {
                var car = await _carProvider.GetCarByIdAsync(carId.Value);
                if (car == null || car.Status != CarStatus.Available)
                {
                    _toast.AddErrorToastMessage("Selected car is not available.");
                    return RedirectToAction("Index", "Car");
                }
                ViewBag.SelectedCar = car;
            }

            ViewBag.AvailableCars = await _carProvider.GetAvailableCarsAsync();
            ViewBag.Customers = await _customerProvider.GetAllCustomersAsync();

            var model = new RentalRequestDTO
            {
                CarId = carId ?? 0,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(1)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Open(RentalRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.AvailableCars = await _carProvider.GetAvailableCarsAsync();
                ViewBag.Customers = await _customerProvider.GetAllCustomersAsync();
                return View(request);
            }

            int employeeId = GetCurrentEmployeeId();
            var result = await _rentalServices.OpenRequestRentalAsync(request, employeeId);
            if (!result.Success)
            {
                _toast.AddErrorToastMessage(result.Content);
                ViewBag.AvailableCars = await _carProvider.GetAvailableCarsAsync();
                ViewBag.Customers = await _customerProvider.GetAllCustomersAsync();
                return View(request);
            }

            await _auditServices.LogAsync("New Rental Contract", "Rentals", $"Employee {User.Identity?.Name} opened new rental contract for Car #{request.CarId} and Customer #{request.CustomerId}", User.Identity?.Name, employeeId);
            _toast.AddSuccessToastMessage("Contract opened & tax invoice generated successfully!");
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int rentalId, string? reason)
        {
            int employeeId = GetCurrentEmployeeId();
            var result = await _rentalServices.CancelRentalAsync(rentalId, employeeId, reason);
            if (!result.Success)
                _toast.AddErrorToastMessage(result.Content);
            else
                _toast.AddSuccessToastMessage(result.Content);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Pay(int rentalId)
        {
            return RedirectToAction("MakePayment", "Payment", new { rentalId = rentalId });
        }

        [HttpGet]
        public async Task<IActionResult> Details(int rentalId)
        {
            int? employeeId = IsUserAdmin() ? null : GetCurrentEmployeeId();
            var rental = await _rentalProvider.GetRentalDetailsByIdAsync(rentalId);
            if (rental == null)
            {
                _toast.AddErrorToastMessage("Rental contract not found.");
                return RedirectToAction("Index");
            }

            var payments = await _paymentServices.GetContractPaymentsAsync(rentalId, employeeId ?? GetCurrentEmployeeId());
            ViewBag.Payments = payments;
            return View(rental);
        }

        [HttpGet]
        public async Task<IActionResult> PrintContract(int rentalId)
        {
            int? employeeId = IsUserAdmin() ? null : GetCurrentEmployeeId();
            var rental = await _rentalProvider.GetRentalDetailsByIdAsync(rentalId);
            if (rental == null)
            {
                _toast.AddErrorToastMessage("Rental contract not found.");
                return RedirectToAction("Index");
            }

            var payments = await _paymentServices.GetContractPaymentsAsync(rentalId, employeeId ?? GetCurrentEmployeeId());
            ViewBag.Payments = payments;
            return View("PrintContract", rental);
        }

        [HttpGet]
        public async Task<IActionResult> Extend(int rentalId)
        {
            var rental = await _rentalProvider.GetRentalDetailsByIdAsync(rentalId);
            if (rental == null)
            {
                _toast.AddErrorToastMessage("Rental contract not found.");
                return RedirectToAction("Index");
            }

            ViewBag.Rental = rental;

            var model = new ExtendRentalDto
            {
                RentalId = rental.Id,
                NewEndDate = rental.EndDate.AddDays(1),
                Notes = rental.Notes
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Extend(ExtendRentalDto extend)
        {
            if (!ModelState.IsValid)
                return View(extend);

            int employeeId = GetCurrentEmployeeId();
            var result = await _rentalServices.ExtendContractAsync(extend, employeeId);
            if (!result.Success)
            {
                _toast.AddErrorToastMessage(result.Content);
                ViewBag.Rental = await _rentalProvider.GetRentalDetailsByIdAsync(extend.RentalId);
                return View(extend);
            }

            _toast.AddSuccessToastMessage(result.Content);
            return RedirectToAction("Details", new { rentalId = result.id });
        }

        [HttpGet]
        public async Task<IActionResult> Close(int rentalId)
        {
            var rental = await _rentalProvider.GetRentalDetailsByIdAsync(rentalId);
            if (rental == null)
            {
                _toast.AddErrorToastMessage("Rental contract not found.");
                return RedirectToAction("Index");
            }
            if (rental.Status != RentalContractStatus.Open)
            {
                _toast.AddWarningToastMessage("Only active open contracts can be closed.");
                return RedirectToAction("Details", new { rentalId = rentalId });
            }

            ViewBag.Rental = rental;

            var model = new RentalCloseDto
            {
                RentalId = rental.Id,
                EndOdometer = 0,
                EndFuelLevel = "Full"
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Close(RentalCloseDto request)
        {
            int employeeId = GetCurrentEmployeeId();
            var result = await _rentalServices.CloseContractAsync(request, employeeId);
            if (!result.Success)
            {
                _toast.AddErrorToastMessage(result.Content);
                ViewBag.Rental = await _rentalProvider.GetRentalDetailsByIdAsync(request.RentalId);
                return View(request);
            }

            _toast.AddSuccessToastMessage(result.Content);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Active(string? searchTerm)
        {
            var activeRentals = await _rentalProvider.GetActiveRentalsAsync(searchTerm);
            var model = new ActiveRentalsViewModel
            {
                Rentals = activeRentals,
                SearchTerm = searchTerm
            };
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Calendar()
        {
            var rentals = await _rentalProvider.GetAllRentalsAsync();
            return View(rentals);
        }
    }
}