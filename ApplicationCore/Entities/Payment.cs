using ApplicationCore.Entities.Enums;


namespace ApplicationCore.Entities
{
  public class Payment:EntityBase
  {
    public int RentalContractId { get; set; }
    public RentalContract RentalContract { get; set; } = null!;

    public decimal Amount { get; set; }
    public PaymentPurpose Purpose { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime PaymentDate { get; set; }
  }
}
