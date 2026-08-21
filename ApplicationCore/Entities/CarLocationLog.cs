namespace ApplicationCore.Entities
{
    public class CarLocationLog : EntityBase
    {
        public int CarId { get; set; }
        public Car Car { get; set; } = null!;

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double SpeedKmh { get; set; }
        public string? AddressName { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool IsEngineOn { get; set; } = true;
    }
}
