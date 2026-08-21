namespace Application.DTOs
{
    public class AuditLogDto
    {
        public int Id { get; set; }
        public int? EmployeeId { get; set; }
        public string EmployeeName { get; set; } = null!;
        public string Action { get; set; } = null!;
        public string Module { get; set; } = null!;
        public string Details { get; set; } = null!;
        public string? IpAddress { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
