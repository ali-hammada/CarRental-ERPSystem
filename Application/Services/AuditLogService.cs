using ApplicationCore.Entities;
using InFrastructure.Data;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Application.Services
{
    public interface IAuditLogService
    {
        Task LogActionAsync(int? employeeId, string? employeeName, string action, string module, string details, string? ipAddress = null);
    }

    public class AuditLogService : IAuditLogService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditLogService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogActionAsync(int? employeeId, string? employeeName, string action, string module, string details, string? ipAddress = null)
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
                    EmployeeId = employeeId,
                    EmployeeName = resolvedName,
                    Action = action,
                    Module = module,
                    Details = details,
                    IpAddress = ipAddress ?? httpContext?.Connection?.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                    Timestamp = DateTime.UtcNow
                };

                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();
            }
            catch
            {
                // Silently handle audit logging errors so operational workflows are never blocked
            }
        }
    }
}
