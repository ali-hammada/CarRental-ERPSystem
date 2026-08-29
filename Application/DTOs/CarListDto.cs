using ApplicationCore.Enums;

namespace Application.DTOs
{
    public class CarListDto
    {
        public int Id { get; set; }
        public string PlateNumber { get; set; } = null!;
        public string Model { get; set; } = null!;
        public int Year { get; set; }
        public decimal PricePerDay { get; set; }
        public CarStatus Status { get; set; }
        public string? ImageUrl { get; set; }
        public string CategoryName { get; set; } = "General";
        public int CurrentOdometer { get; set; }
        public string FuelType { get; set; } = "Gasoline";
        public string Transmission { get; set; } = "Automatic";
        public string? Color { get; set; }
        public DateTime? LicenseExpiryDate { get; set; }
        public DateTime? InsuranceExpiryDate { get; set; }

        // Dealership Sourcing & Procurement
        public CarListingType ListingType { get; set; } = CarListingType.RentalOnly;
        public decimal? SalePrice { get; set; }
        public CarSaleStatus? SaleStatus { get; set; }
        public decimal? PurchasePrice { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public decimal? RefurbishmentCost { get; set; }
        public decimal TotalCostBasis => (PurchasePrice ?? 0m) + (RefurbishmentCost ?? 0m);
        public decimal? TargetSalePrice { get; set; }
        public decimal? MinimumFloorPrice { get; set; }

        public string? OriginalPurchaseContractUrl { get; set; }
        public string? FinalBuyerContractUrl { get; set; }
    }
}
