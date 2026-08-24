using ApplicationCore.Enums;

namespace ApplicationCore.Entities
{
    public class SaleInstallment : EntityBase
    {
        public int SaleContractId { get; set; }
        public CarSaleContract SaleContract { get; set; } = null!;

        public int InstallmentNumber { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public decimal PaidAmount { get; set; } = 0;
        public DateTime? PaidDate { get; set; }

        public InstallmentStatus Status { get; set; } = InstallmentStatus.Pending;
        public string? TransactionReference { get; set; }
    }
}
