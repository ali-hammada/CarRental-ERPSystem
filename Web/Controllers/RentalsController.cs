using Application.DTOs;
using Application.Providers;
using Application.Services;
using Application.Services.DTOs;
using ApplicationCore.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public RentalsController(
            IRentalProvider rentalProvider,
            ICarProvider carProvider,
            ICustomerProvider customerProvider,
            IRentalServices rentalServices,
            IPaymentServices paymentServices,
            IAuditServices auditServices)
        {
            _rentalProvider = rentalProvider;
            _carProvider = carProvider;
            _customerProvider = customerProvider;
            _rentalServices = rentalServices;
            _paymentServices = paymentServices;
            _auditServices = auditServices;
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

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var rentals = await _rentalProvider.GetAllRentalsAsync();
            return View(rentals);
        }

        [HttpGet]
        public async Task<IActionResult> Open(int? carId, int? customerId)
        {
            ViewBag.AvailableCars = await _carProvider.GetAvailableCarsAsync();
            ViewBag.Customers = await _customerProvider.GetAllCustomersAsync();

            var model = new RentalRequestDTO
            {
                CarId = carId ?? 0,
                CustomerId = customerId ?? 0,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(3)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Open(RentalRequestDTO request)
        {
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (!ModelState.IsValid)
            {
                if (isAjax)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join("<br>", errors) });
                }
                ViewBag.AvailableCars = await _carProvider.GetAvailableCarsAsync();
                ViewBag.Customers = await _customerProvider.GetAllCustomersAsync();
                return View(request);
            }

            int employeeId = GetCurrentEmployeeId();
            var result = await _rentalServices.OpenRequestRentalAsync(request, employeeId);
            if (!result.Success)
            {
                if (isAjax) return Json(new { success = false, message = result.Content });
                ViewBag.AvailableCars = await _carProvider.GetAvailableCarsAsync();
                ViewBag.Customers = await _customerProvider.GetAllCustomersAsync();
                return View(request);
            }

            await _auditServices.LogAsync("New Rental Contract", "Rentals", $"Employee {User.Identity?.Name} opened new rental contract for Car #{request.CarId} and Customer #{request.CustomerId}", User.Identity?.Name, employeeId);

            if (isAjax) return Json(new { success = true, redirectUrl = Url.Action("Index") });
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int rentalId, string? reason)
        {
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            int employeeId = GetCurrentEmployeeId();
            var result = await _rentalServices.CancelRentalAsync(rentalId, employeeId, reason);
            if (isAjax) return Json(new { success = result.Success, message = result.Content, redirectUrl = Url.Action("Index") });
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
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (!ModelState.IsValid)
            {
                if (isAjax)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join("<br>", errors) });
                }
                return View(extend);
            }

            int employeeId = GetCurrentEmployeeId();
            var result = await _rentalServices.ExtendContractAsync(extend, employeeId);
            if (!result.Success)
            {
                if (isAjax) return Json(new { success = false, message = result.Content });
                ViewBag.Rental = await _rentalProvider.GetRentalDetailsByIdAsync(extend.RentalId);
                return View(extend);
            }

            if (isAjax) return Json(new { success = true, redirectUrl = Url.Action("Details", new { rentalId = result.id }) });
            return RedirectToAction("Details", new { rentalId = result.id });
        }

        [HttpGet]
        public async Task<IActionResult> Close(int rentalId)
        {
            var rental = await _rentalProvider.GetRentalDetailsByIdAsync(rentalId);
            if (rental == null)
            {
                return RedirectToAction("Index");
            }
            if (rental.Status != RentalContractStatus.Open)
            {
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
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            int employeeId = GetCurrentEmployeeId();
            var result = await _rentalServices.CloseContractAsync(request, employeeId);
            if (!result.Success)
            {
                if (isAjax) return Json(new { success = false, message = result.Content });
                ViewBag.Rental = await _rentalProvider.GetRentalDetailsByIdAsync(request.RentalId);
                return View(request);
            }

            if (isAjax) return Json(new { success = true, redirectUrl = Url.Action("Index") });
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