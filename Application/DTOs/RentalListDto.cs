using ApplicationCore.Enums;

namespace Application.DTOs
{
    public class RentalListDto
    {
        public int Id { get; set; }
        public int CarId { get; set; }
        public string CarModel { get; set; } = null!;
        public string CarPlateNumber { get; set; } = null!;

        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = null!;
        public string CustomerPhone { get; set; } = null!;

        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = null!;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? ActualEndDate { get; set; }

        public decimal DailyPrice { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal? FinalAmount { get; set; }
        public decimal? ExtraFees { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount => (FinalAmount ?? TotalAmount) - PaidAmount;

        public RentalContractStatus Status { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
