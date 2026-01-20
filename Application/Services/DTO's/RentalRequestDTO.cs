namespace Application.Services.DTO_s
{
  public class RentalRequestDTO
  {
    public int RentalId { get; set; }
    public int CarId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public string Notes { get; set; }
  }
}
