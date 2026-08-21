namespace Application.Services.DTOs
{
    public class RentalRequestDTO
    {
        public int RentalId { get; set; }
        public int CarId { get; set; }
        public int CustomerId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int StartOdometer { get; set; } = 0;
        public string StartFuelLevel { get; set; } = "Full";
        public decimal DepositAmount { get; set; } = 0;
        public string? Notes { get; set; }
    }
}
