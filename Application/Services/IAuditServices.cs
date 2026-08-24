namespace Application.Services
{
    public interface IAuditServices
    {
        Task LogAsync(string action, string module, string details, string? employeeName = null, int? employeeId = null, string? ipAddress = null);
    }
}
