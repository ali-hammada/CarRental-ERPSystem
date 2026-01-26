using ApplicationCore.Entities.Enums;

namespace ApplicationCore.Entities
{
  public class Car:EntityBase
  {
    public string PlateNumber { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public decimal PricePerDay { get; set; }
    public CarStatus Status { get; set; }
<<<<<<< HEAD
    public string? ImageUrl { get; set; }
=======
    public byte[]? ImageUrl { get; set; }
>>>>>>> bb34bdc6388bf2d2af71bac74f7c565120e5082e


    public ICollection<RentalContract> RentalContracts { get; set; } = new List<RentalContract>();
  }
}
