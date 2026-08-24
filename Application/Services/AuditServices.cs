using ApplicationCore.Entities;
using InFrastructure.Data;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Application.Services
{
    public class AuditServices : IAuditServices
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditServices(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(string action, string module, string details, string? employeeName = null, int? employeeId = null, string? ipAddress = null)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var currentUserName = httpContext?.User?.Identity?.Name;
                if (string.IsNullOrWhiteSpace(currentUserName))
                {
                    currentUserName = httpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;
                }

                string resolvedName = !string.IsNullOrWhiteSpace(currentUserName)
                    ? currentUserName
                    : (!string.IsNullOrWhiteSpace(employeeName) && employeeName != "Staff User" ? employeeName : "Admin");

                var auditLog = new AuditLog
                {
                    Action = action,
                    Module = module,
                    Details = details,
                    EmployeeName = resolvedName,
                    EmployeeId = employeeId,
                    IpAddress = ipAddress ?? httpContext?.Connection?.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                    Timestamp = DateTime.UtcNow
                };

                await _context.AuditLogs.AddAsync(auditLog);
                await _context.SaveChangesAsync();
            }
            catch
            {
                // Silently handle logging exceptions
            }
        }
    }
}
