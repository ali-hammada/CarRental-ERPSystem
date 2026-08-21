namespace ApplicationCore.Entities
{
    public class Invoice : EntityBase
    {
        public string InvoiceNumber { get; set; } = null!;
        public int RentalContractId { get; set; }
        public RentalContract RentalContract { get; set; } = null!;

        public DateTime IssueDate { get; set; } = DateTime.UtcNow;
        public decimal SubTotal { get; set; }
        public decimal TaxRate { get; set; } = 0.14m; // 14% VAT default
        public decimal TaxAmount { get; set; }
        public decimal ExtraFees { get; set; } = 0;
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount => TotalAmount - PaidAmount;

        public string Status { get; set; } = "Issued"; // Draft, Issued, Paid, Cancelled
    }
}
