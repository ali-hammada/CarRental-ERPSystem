using Application.Providers;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;
using Web.Resources;

namespace Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IDashboardProvider _dashboardProvider;
        private readonly IPaymentServices _paymentServices;
        private readonly IStringLocalizer<SharedResources> _localizer;

        public HomeController(
            IDashboardProvider dashboardProvider,
            IPaymentServices paymentServices,
            IStringLocalizer<SharedResources> localizer)
        {
            _dashboardProvider = dashboardProvider;
            _paymentServices = paymentServices;
            _localizer = localizer;
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

        public IActionResult Index()
        {
            ViewData["Welcome"] = _localizer["Welcome"];
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpGet]
        public async Task<IActionResult> GetPartialRecentPayments(int page = 1, int pageSize = 5)
        {
            int? employeeId = IsUserAdmin() ? null : GetCurrentEmployeeId();
            var metrics = await _dashboardProvider.GetDashboardMetricsAsync(employeeId);
            return PartialView("_RecentPaymentsPartial", metrics);
        }

        public async Task<IActionResult> Dashboard()
        {
            int? employeeId = IsUserAdmin() ? null : GetCurrentEmployeeId();
            var metrics = await _dashboardProvider.GetDashboardMetricsAsync(employeeId);
            return View(metrics);
        }
    }
}