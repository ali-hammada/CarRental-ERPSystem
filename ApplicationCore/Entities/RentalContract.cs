using ApplicationCore.Entities.Enums;

namespace ApplicationCore.Entities
{
  public class RentalContract:EntityBase
  {
    public int CarId { get; set; }
    public int CustomerId { get; set; }
    public Car Car { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal DailyPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public RentalContractStatus Status { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public decimal? FinalAmount { get; set; }
    public decimal? ExtraFees { get; set; }
    public string Notes { get; set; } = null!;
  }

}
