using ApplicationCore.Entities;
using InFrastructure.Data;

namespace Application.Services
{
    public interface IAuditLogService
    {
        Task LogActionAsync(int? employeeId, string employeeName, string action, string module, string details, string? ipAddress = null);
    }

    public class AuditLogService : IAuditLogService
    {
        private readonly AppDbContext _context;

        public AuditLogService(AppDbContext context)
        {
            _context = context;
        }

        public async Task LogActionAsync(int? employeeId, string employeeName, string action, string module, string details, string? ipAddress = null)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    EmployeeId = employeeId,
                    EmployeeName = string.IsNullOrWhiteSpace(employeeName) ? "System User" : employeeName,
                    Action = action,
                    Module = module,
                    Details = details,
                    IpAddress = ipAddress ?? "127.0.0.1",
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
