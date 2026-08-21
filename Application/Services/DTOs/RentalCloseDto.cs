namespace Application.Services.DTOs
{
    public class RentalCloseDto
    {
        public int RentalId { get; set; }
        public int EndOdometer { get; set; }
        public string EndFuelLevel { get; set; } = "Full";
        public decimal ExtraFees { get; set; } = 0;
        public string? DamageNotes { get; set; }
        public string? Notes { get; set; }
    }
}
