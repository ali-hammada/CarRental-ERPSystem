using ApplicationCore.Entities.Enums;

namespace ApplicationCore.Entities
{
  public class RentalContract:EntityBase
  {
    public int CarId { get; set; }
    public Car Car { get; set; } = null!;

    public int EmployeeId { get; set; }
    public Employees Employee { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal DailyPrice { get; set; }
    public decimal TotalAmount { get; set; }

    public RentalContractStatus Status { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public decimal? FinalAmount { get; set; }
    public decimal? ExtraFees { get; set; }

    public decimal PaidAmount { get; set; } = 0;
    public decimal RemainingAmount => (FinalAmount??TotalAmount)-PaidAmount;

    public string Notes { get; set; } = null!;
    public PaymentStatus PaymentStatus { get; set; }

    // 1-N Payments
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
  }

}
