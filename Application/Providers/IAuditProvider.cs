using ApplicationCore.Entities;

namespace Application.Providers
{
    public interface IAuditProvider
    {
        Task<List<AuditLog>> GetRecentAuditLogsAsync(int count = 10);
        Task<List<AuditLog>> GetAllAuditLogsAsync();
    }
}
