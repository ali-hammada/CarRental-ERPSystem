using ApplicationCore.Entities;
using InFrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Providers
{
    public class AuditProvider : IAuditProvider
    {
        private readonly AppDbContext _context;

        public AuditProvider(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<AuditLog>> GetRecentAuditLogsAsync(int count = 10)
        {
            return await _context.AuditLogs
                .AsNoTracking()
                .OrderByDescending(a => a.Timestamp)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetAllAuditLogsAsync()
        {
            return await _context.AuditLogs
                .AsNoTracking()
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }
    }
}
