using Application.Providers;
using Application.Services;
using ApplicationCore.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Web.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly IPaymentServices _paymentService;
        private readonly IRentalProvider _rentalProvider;

        public PaymentController(IPaymentServices paymentServices, IRentalProvider rentalProvider)
        {
            _paymentService = paymentServices;
            _rentalProvider = rentalProvider;
        }

        private bool IsUserAdmin()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var name = User.Identity?.Name;
            return User.IsInRole("Admin") || User.IsInRole("Administrator") ||
                   string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase) ||
                   (!string.IsNullOrEmpty(name) && name.Contains("admin", StringComparison.OrdinalIgnoreCase));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            List<Application.DTOs.RentalListDto> rentals;
            if (IsUserAdmin())
            {
                rentals = await _rentalProvider.GetAllRentalsAsync();
            }
            else
            {
                int employeeId = GetCurrentEmployeeId();
                rentals = await _rentalProvider.GetEmployeeRentalsAsync(employeeId);
            }

            var unpaidRentals = rentals.Where(r =>
                r.Status == RentalContractStatus.Open ||
                (r.FinalAmount ?? r.TotalAmount) > r.PaidAmount
            ).ToList();
            return View(unpaidRentals);
        }

        [HttpGet]
        public async Task<IActionResult> MakePayment(int rentalId)
        {
            int? employeeId = IsUserAdmin() ? null : GetCurrentEmployeeId();
            var (success, message, remaining) = await _paymentService.GetRemainingAmountAsync(rentalId, employeeId ?? GetCurrentEmployeeId());
            if (!success)
            {
                return RedirectToAction(nameof(Index));
            }
            ViewBag.RentalId = rentalId;
            ViewBag.RemainingAmount = remaining;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MakePayment(int rentalId, decimal amount, PaymentPurpose purpose, PaymentMethod method, string? transactionRef = null)
        {
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            int? employeeId = IsUserAdmin() ? null : GetCurrentEmployeeId();
            var result = await _paymentService.MakePaymentAsync(rentalId, amount, purpose, method, employeeId ?? GetCurrentEmployeeId(), transactionRef);
            if (!result.success)
            {
                if (isAjax) return Json(new { success = false, message = result.message });
                ViewBag.RentalId = rentalId;
                ViewBag.RemainingAmount = amount;
                return View();
            }

            if (isAjax) return Json(new { success = true, redirectUrl = Url.Action("Receipt", new { paymentId = result.PaymentId }) });
            return RedirectToAction("Receipt", new { paymentId = result.PaymentId });
        }

        [HttpGet]
        public async Task<IActionResult> History(int? rentalId = null)
        {
            int? employeeId = IsUserAdmin() ? null : GetCurrentEmployeeId();
            if (rentalId.HasValue)
            {
                var payments = await _paymentService.GetContractPaymentsAsync(rentalId.Value, employeeId ?? GetCurrentEmployeeId());
                ViewBag.RentalId = rentalId.Value;
                return View(payments);
            }
            else
            {
                var allPayments = await _paymentService.GetAllEmployeesPaymentsAsync(employeeId ?? GetCurrentEmployeeId());
                return View("AllPayments", allPayments);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Receipt(int paymentId)
        {
            int? employeeId = IsUserAdmin() ? null : GetCurrentEmployeeId();
            var payment = await _paymentService.GetPaymentByIdAsync(paymentId, employeeId ?? GetCurrentEmployeeId());
            if (payment == null)
            {
                return RedirectToAction(nameof(Index));
            }
            return View(payment);
        }

        private int GetCurrentEmployeeId()
        {
            var employeeIdClaim = User.FindFirst("EmployeeId")?.Value
                               ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(employeeIdClaim))
                throw new UnauthorizedAccessException("User is not authenticated.");
            return int.Parse(employeeIdClaim);
        }
    }
}