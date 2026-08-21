using Application.DTOs;
using InFrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Providers
{
    public interface IAuditLogProvider
    {
        Task<List<AuditLogDto>> GetAllAuditLogsAsync(string? module = null, string? searchTerm = null);
        Task<List<AuditLogDto>> GetEmployeeAuditLogsAsync(int employeeId);
    }

    public class AuditLogProvider : IAuditLogProvider
    {
        private readonly AppDbContext _context;

        public AuditLogProvider(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<AuditLogDto>> GetAllAuditLogsAsync(string? module = null, string? searchTerm = null)
        {
            var query = _context.AuditLogs.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(module))
            {
                query = query.Where(a => a.Module.ToLower() == module.ToLower());
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(a =>
                    a.EmployeeName.Contains(searchTerm) ||
                    a.Action.Contains(searchTerm) ||
                    a.Details.Contains(searchTerm) ||
                    a.Module.Contains(searchTerm));
            }

            return await query
                .OrderByDescending(a => a.Timestamp)
                .Select(a => new AuditLogDto
                {
                    Id = a.Id,
                    EmployeeId = a.EmployeeId,
                    EmployeeName = a.EmployeeName,
                    Action = a.Action,
                    Module = a.Module,
                    Details = a.Details,
                    IpAddress = a.IpAddress,
                    Timestamp = a.Timestamp
                })
                .ToListAsync();
        }

        public async Task<List<AuditLogDto>> GetEmployeeAuditLogsAsync(int employeeId)
        {
            return await _context.AuditLogs
                .AsNoTracking()
                .Where(a => a.EmployeeId == employeeId)
                .OrderByDescending(a => a.Timestamp)
                .Select(a => new AuditLogDto
                {
                    Id = a.Id,
                    EmployeeId = a.EmployeeId,
                    EmployeeName = a.EmployeeName,
                    Action = a.Action,
                    Module = a.Module,
                    Details = a.Details,
                    IpAddress = a.IpAddress,
                    Timestamp = a.Timestamp
                })
                .ToListAsync();
        }
    }
}
