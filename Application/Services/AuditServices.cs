using ApplicationCore.Entities;
using InFrastructure.Data;

namespace Application.Services
{
    public class AuditServices : IAuditServices
    {
        private readonly AppDbContext _context;

        public AuditServices(AppDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(string action, string module, string details, string? employeeName = null, int? employeeId = null, string? ipAddress = null)
        {
            var auditLog = new AuditLog
            {
                Action = action,
                Module = module,
                Details = details,
                EmployeeName = string.IsNullOrWhiteSpace(employeeName) ? "System / Employee" : employeeName,
                EmployeeId = employeeId,
                IpAddress = ipAddress,
                Timestamp = DateTime.UtcNow
            };

            await _context.AuditLogs.AddAsync(auditLog);
            await _context.SaveChangesAsync();
        }
    }
}
