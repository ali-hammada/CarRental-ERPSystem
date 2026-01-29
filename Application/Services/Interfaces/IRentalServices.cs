using Application.Services.DTO_s;
using ApplicationCore.Entities;

namespace Application.Services.Interfaces
{
  public interface IRentalServices
  {
    Task<RentalContract> GetRentalByIdAsync(int rentalId);
    Task<(bool Success, string Content, int id)> OpenRequestRentalAsync(RentalRequestDTO request,int customerId);
    Task<(bool Success, string Content, int id)> CancelRentalAsync(int rentalId,int customerId,string? reason = null);
    Task<(bool Success, string Content, int id)> ExtendContractAsync(ExtendRentalDto extend,int customerId);
    Task<(bool Success, string Content, int id)> CloseContractAsync(RentalCloseDto request,int customerId);

    Task<bool> HasActiveRentalAsync(int carId,DateTime start,DateTime end);
    Task<List<RentalContract>> GetEmployeesRentalsAsync(int customerId);
    Task<List<RentalContract>> GetEmployeesRentalsWithCarsAsync(int employeeId);
  }
}
