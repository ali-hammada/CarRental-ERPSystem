using ApplicationCore.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class CarSaleListDto
    {
        public int Id { get; set; }
        public int CarId { get; set; }
        public string CarModel { get; set; } = null!;
        public string CarPlateNumber { get; set; } = null!;
        
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = null!;
        public string CustomerPhone { get; set; } = null!;

        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = null!;

        public DateTime SaleDate { get; set; }
        public decimal SalePrice { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal FinalPrice { get; set; }

        public SalePaymentType PaymentType { get; set; }
        public decimal DownPayment { get; set; }
        public int InstallmentMonths { get; set; }
        public decimal MonthlyInstallment { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingBalance => FinalPrice - PaidAmount;

        // Profitability Breakdown
        public decimal TotalCostBasis { get; set; }
        public decimal ActualGrossProfit { get; set; }
        public bool IsBelowFloorPrice { get; set; }

        public SaleContractStatus Status { get; set; }
        public string? Notes { get; set; }
    }

    public class CarSaleRequestDto
    {
        [Required]
        public int CarId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        [Range(1, 100000000, ErrorMessage = "Sale price must be positive.")]
        public decimal SalePrice { get; set; }

        public decimal TaxRatePercent { get; set; } = 14; // Default 14% VAT

        public SalePaymentType PaymentType { get; set; } = SalePaymentType.Cash;

        public decimal DownPayment { get; set; } = 0;

        [Range(0, 120, ErrorMessage = "Installment duration between 0 and 120 months.")]
        public int InstallmentMonths { get; set; } = 0;

        public string? Notes { get; set; }
    }

    public class SaleInstallmentDto
    {
        public int Id { get; set; }
        public int SaleContractId { get; set; }
        public int InstallmentNumber { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public decimal PaidAmount { get; set; }
        public DateTime? PaidDate { get; set; }
        public InstallmentStatus Status { get; set; }
        public string? TransactionReference { get; set; }
    }

    public class CarSaleNegotiationMetadataDto
    {
        public int CarId { get; set; }
        public string CarModel { get; set; } = null!;
        public string CarPlateNumber { get; set; } = null!;
        public decimal PurchasePrice { get; set; }
        public decimal RefurbishmentCost { get; set; }
        public decimal TotalCostBasis { get; set; }
        public decimal TargetSalePrice { get; set; }
        public decimal MinimumFloorPrice { get; set; }
    }
}
