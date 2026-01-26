using ApplicationCore.Entities;
using ApplicationCore.Entities.Enums;

namespace Application.Services.DTO_s
{
  public class PaymentDTO:EntityBase
  {

    public int RentalContractId { get; set; }
    public decimal Amount { get; set; }
    public PaymentPurpose Purpose { get; set; }
    public PaymentMethod Method { get; set; }
    public DateTime PaymentDate { get; set; }

  }
}
