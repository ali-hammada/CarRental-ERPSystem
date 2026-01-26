using ApplicationCore.Entities;

namespace Web.ViewModels
{
  public class ActiveRentalsViewModel
  {

    public IEnumerable<RentalContract> Rentals { get; set; } = new List<RentalContract>();

    public string? SearchTerm { get; set; }
  }
}
