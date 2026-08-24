using ApplicationCore.Enums;

namespace ApplicationCore.Entities
{
    public class CarSaleContract : EntityBase
    {
        public int CarId { get; set; }
        public Car Car { get; set; } = null!;

        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        public DateTime SaleDate { get; set; } = DateTime.UtcNow;
        public decimal SalePrice { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal FinalPrice { get; set; }

        public SalePaymentType PaymentType { get; set; } = SalePaymentType.Cash;

        // Dealership Profitability Tracking
        public decimal TotalCostBasis { get; set; } = 0;
        public decimal ActualGrossProfit { get; set; } = 0;
        public bool IsBelowFloorPrice { get; set; } = false;

        // Installment Details
        public decimal DownPayment { get; set; } = 0;
        public int InstallmentMonths { get; set; } = 0;
        public decimal MonthlyInstallment { get; set; } = 0;
        public decimal PaidAmount { get; set; } = 0;

        public SaleContractStatus Status { get; set; } = SaleContractStatus.Completed;
        public string? Notes { get; set; }

        public ICollection<SaleInstallment> Installments { get; set; } = new List<SaleInstallment>();
    }
}
