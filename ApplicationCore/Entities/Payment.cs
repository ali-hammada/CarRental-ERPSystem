namespace ApplicationCore.Entities
{
  public class Payment:EntityBase
  {
    public int RentalContractId { get; set; }
    public RentalContract RentalContract { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string Method { get; set; } = null!;
  }
}
