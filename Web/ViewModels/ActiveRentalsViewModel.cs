using Application.DTOs;

namespace Web.ViewModels
{
    public class ActiveRentalsViewModel
    {
        public IEnumerable<RentalListDto> Rentals { get; set; } = new List<RentalListDto>();
        public string? SearchTerm { get; set; }
    }
}
