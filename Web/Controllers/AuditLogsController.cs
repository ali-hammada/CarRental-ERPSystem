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
    }
}
