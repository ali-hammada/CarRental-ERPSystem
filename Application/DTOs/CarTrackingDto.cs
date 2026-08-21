using ApplicationCore.Enums;

namespace Application.DTOs
{
    public class CarTrackingDto
    {
        public int CarId { get; set; }
        public string Model { get; set; } = null!;
        public string PlateNumber { get; set; } = null!;
        public CarStatus Status { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double SpeedKmh { get; set; }
        public string AddressName { get; set; } = "Current Vehicle Coordinates";
        public DateTime LastUpdated { get; set; }
        public bool IsEngineOn { get; set; }

        public string? ActiveCustomerName { get; set; }
        public string? ActiveContractNumber { get; set; }
    }

    public class LocationHistoryDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double SpeedKmh { get; set; }
        public string AddressName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public bool IsEngineOn { get; set; }
    }
}
