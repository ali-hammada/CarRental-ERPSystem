using Application.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [Authorize]
    public class AuditLogsController : Controller
    {
        private readonly IAuditLogProvider _auditLogProvider;

        public AuditLogsController(IAuditLogProvider auditLogProvider)
        {
            _auditLogProvider = auditLogProvider;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? module, string? searchTerm)
        {
            var logs = await _auditLogProvider.GetAllAuditLogsAsync(module, searchTerm);
            ViewBag.SelectedModule = module;
            ViewBag.SearchTerm = searchTerm;
            return View(logs);
        }

        [HttpGet]
        public async Task<IActionResult> GetLatestAudit()
        {
            var logs = await _auditLogProvider.GetAllAuditLogsAsync(null, null);
            var latest = logs.FirstOrDefault();
            if (latest == null) return Json(null);

            return Json(new
            {
                id = latest.Id,
                employeeName = latest.EmployeeName,
                action = latest.Action,
                module = latest.Module,
                details = latest.Details,
                time = latest.Timestamp.ToString("HH:mm:ss")
            });
        }
    }
}
