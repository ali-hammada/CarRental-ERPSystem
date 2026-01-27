using ApplicationCore.Entities.Enums;

namespace ApplicationCore.Entities
{
  public class Car:EntityBase
  {
    public string PlateNumber { get; set; } = null!;
    public string Model { get; set; } = null!;
    public int Year { get; set; }
    public decimal PricePerDay { get; set; }

    public CarStatus Status { get; set; }
    public byte[]? ImageUrl { get; set; }


    public ICollection<RentalContract> RentalContracts { get; set; } = new List<RentalContract>();
  }
}
