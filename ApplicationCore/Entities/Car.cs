using ApplicationCore.Enums;

namespace ApplicationCore.Entities
{
    public class Car : EntityBase
    {
        public string PlateNumber { get; set; } = null!;
        public string Model { get; set; } = null!;
        public int Year { get; set; }
        public decimal PricePerDay { get; set; }
        public CarStatus Status { get; set; } = CarStatus.Available;
        public string? ImageUrl { get; set; }

        public int? CategoryId { get; set; }
        public CarCategory? Category { get; set; }

        public int CurrentOdometer { get; set; } = 0;
        public string FuelType { get; set; } = "Gasoline";
        public string Transmission { get; set; } = "Automatic";
        public string? Color { get; set; }

        // Live GPS Telemetry
        public double? CurrentLatitude { get; set; }
        public double? CurrentLongitude { get; set; }
        public DateTime? LastLocationUpdate { get; set; }

        // Vehicle Registration & Insurance Expiry Tracking
        public DateTime? LicenseExpiryDate { get; set; }
        public DateTime? InsuranceExpiryDate { get; set; }

        // Sales & Dealership Properties
        public CarListingType ListingType { get; set; } = CarListingType.RentalOnly;
        public decimal? SalePrice { get; set; }
        public CarSaleStatus? SaleStatus { get; set; }

        // Dealership Sourcing & Procurement Properties
        public decimal? PurchasePrice { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public decimal? RefurbishmentCost { get; set; }
        public decimal TotalCostBasis => (PurchasePrice ?? 0m) + (RefurbishmentCost ?? 0m);
        public decimal? TargetSalePrice { get; set; }
        public decimal? MinimumFloorPrice { get; set; }

        // Legal & Financial Document Attachments
        public string? OriginalPurchaseContractUrl { get; set; }
        public string? FinalBuyerContractUrl { get; set; }

        public ICollection<RentalContract> RentalContracts { get; set; } = new List<RentalContract>();
        public ICollection<MaintenanceLog> MaintenanceLogs { get; set; } = new List<MaintenanceLog>();
        public ICollection<CarLocationLog> LocationLogs { get; set; } = new List<CarLocationLog>();
        public ICollection<CarSaleContract> SaleContracts { get; set; } = new List<CarSaleContract>();
    }
}
