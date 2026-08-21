using ApplicationCore.Enums;

namespace ApplicationCore.Entities
{
    public class Payment : EntityBase
    {
        public int RentalContractId { get; set; }
        public RentalContract RentalContract { get; set; } = null!;

        public decimal Amount { get; set; }
        public PaymentPurpose Purpose { get; set; }
        public PaymentMethod Method { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Paid;
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public string? TransactionReference { get; set; }
        public string? Notes { get; set; }
    }
}
