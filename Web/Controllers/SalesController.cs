using Application.DTOs;
using Application.Providers;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Web.Controllers
{
    [Authorize]
    public class SalesController : Controller
    {
        private readonly ISaleProvider _saleProvider;
        private readonly ISaleServices _saleServices;
        private readonly ICustomerProvider _customerProvider;
        private readonly IAuditServices _auditServices;

        public SalesController(
            ISaleProvider saleProvider,
            ISaleServices saleServices,
            ICustomerProvider customerProvider,
            IAuditServices auditServices)
        {
            _saleProvider = saleProvider;
            _saleServices = saleServices;
            _customerProvider = customerProvider;
            _auditServices = auditServices;
        }

        private bool IsUserAdmin()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var name = User.Identity?.Name;
            return User.IsInRole("Admin") || User.IsInRole("Administrator") ||
                   string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase) ||
                   (!string.IsNullOrEmpty(name) && name.Contains("admin", StringComparison.OrdinalIgnoreCase));
        }

        private int GetCurrentEmployeeId()
        {
            var employeeIdClaim = User.FindFirst("EmployeeId")?.Value
                                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(employeeIdClaim))
                throw new UnauthorizedAccessException("User is not authenticated.");
            return int.Parse(employeeIdClaim);
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var sales = await _saleProvider.GetAllSaleContractsAsync();
            return View(sales);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? carId)
        {
            ViewBag.AvailableCars = await _saleProvider.GetCarsAvailableForSaleAsync(carId);
            ViewBag.Customers = await _customerProvider.GetAllCustomersAsync();

            var model = new CarSaleRequestDto
            {
                CarId = carId ?? 0,
                TaxRatePercent = 14
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CarSaleRequestDto request)
        {
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (!ModelState.IsValid)
            {
                if (isAjax)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join("<br>", errors) });
                }
                ViewBag.AvailableCars = await _saleProvider.GetCarsAvailableForSaleAsync(request.CarId);
                ViewBag.Customers = await _customerProvider.GetAllCustomersAsync();
                return View(request);
            }

            int employeeId = GetCurrentEmployeeId();
            var result = await _saleServices.CreateSaleContractAsync(request, employeeId);
            if (!result.Success)
            {
                if (isAjax) return Json(new { success = false, message = result.Message });
                ViewBag.AvailableCars = await _saleProvider.GetCarsAvailableForSaleAsync(request.CarId);
                ViewBag.Customers = await _customerProvider.GetAllCustomersAsync();
                return View(request);
            }

            await _auditServices.LogAsync("Vehicle Sale Contract", "Sales", $"Employee {User.Identity?.Name} finalized vehicle sale for Car #{request.CarId} at agreed price ${request.SalePrice}", User.Identity?.Name, employeeId);

            if (isAjax) return Json(new { success = true, redirectUrl = Url.Action(nameof(Details), new { id = result.ContractId }) });
            return RedirectToAction(nameof(Details), new { id = result.ContractId });
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var sale = await _saleProvider.GetSaleContractDetailsByIdAsync(id);
            if (sale == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var installments = await _saleProvider.GetContractInstallmentsAsync(id);
            ViewBag.Installments = installments;
            return View(sale);
        }

        [HttpGet]
        public async Task<IActionResult> PrintSaleAgreement(int id)
        {
            var sale = await _saleProvider.GetSaleContractDetailsByIdAsync(id);
            if (sale == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var installments = await _saleProvider.GetContractInstallmentsAsync(id);
            ViewBag.Installments = installments;
            return View(sale);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayInstallment(int installmentId, int contractId, string? transactionRef)
        {
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            var result = await _saleServices.PayInstallmentAsync(installmentId, transactionRef);
            if (result.Success)
            {
                await _auditServices.LogAsync("Installment Collected", "Sales", $"Employee {User.Identity?.Name} collected installment #{installmentId} for Sale Contract #{contractId}", User.Identity?.Name);
            }

            if (isAjax) return Json(new { success = result.Success, message = result.Message, redirectUrl = Url.Action(nameof(Details), new { id = contractId }) });
            return RedirectToAction(nameof(Details), new { id = contractId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int contractId, string? reason)
        {
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            var result = await _saleServices.CancelSaleContractAsync(contractId, reason);
            if (isAjax) return Json(new { success = result.Success, message = result.Message, redirectUrl = Url.Action(nameof(Index)) });
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Calculator()
        {
            return View();
        }
    }
}
