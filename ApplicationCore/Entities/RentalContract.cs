using ApplicationCore.Enums;

namespace ApplicationCore.Entities
{
    public class RentalContract : EntityBase
    {
        public int CarId { get; set; }
        public Car Car { get; set; } = null!;

        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? ActualEndDate { get; set; }

        public decimal DailyPrice { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal? FinalAmount { get; set; }
        public decimal? ExtraFees { get; set; }
        public decimal DepositAmount { get; set; } = 0;

        public decimal PaidAmount { get; set; } = 0;
        public decimal RemainingAmount => (FinalAmount ?? TotalAmount) - PaidAmount;

        public int StartOdometer { get; set; } = 0;
        public int? EndOdometer { get; set; }
        public string StartFuelLevel { get; set; } = "Full";
        public string? EndFuelLevel { get; set; }

        public RentalContractStatus Status { get; set; } = RentalContractStatus.Open;
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
        public string Notes { get; set; } = string.Empty;

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    }
}
