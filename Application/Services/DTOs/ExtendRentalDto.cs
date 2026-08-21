namespace Application.Services.DTOs
{
    public class ExtendRentalDto
    {
        public int RentalId { get; set; }
        public DateTime? NewEndDate { get; set; }
        public string? Notes { get; set; }
    }
}
