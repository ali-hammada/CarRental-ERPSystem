namespace ApplicationCore.Entities
{
    public class MaintenanceLog : EntityBase
    {
        public int CarId { get; set; }
        public Car Car { get; set; } = null!;

        public string ServiceType { get; set; } = null!; // e.g. Oil Change, Tire Replacement, Brake Inspection
        public string? Description { get; set; }
        public decimal Cost { get; set; }
        public DateTime ServiceDate { get; set; } = DateTime.UtcNow;
        public int OdometerReading { get; set; }
        public string? PerformedBy { get; set; }
        public string Status { get; set; } = "Completed"; // Scheduled, InProgress, Completed, Cancelled
    }
}
