namespace ApplicationCore.Entities
{
    public class AuditLog : EntityBase
    {
        public int? EmployeeId { get; set; }
        public string EmployeeName { get; set; } = "System";

        public string Action { get; set; } = null!;
        public string Module { get; set; } = null!;
        public string Details { get; set; } = null!;
        public string? IpAddress { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
