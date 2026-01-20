using Application.Services.DTO_s;

namespace Application.Services.Interfaces
{
  public interface IRentalServices
  {
    Task<(bool Success, string Content, int id)> OpenRequestRentalAsync(RentalRequestDTO request,int customerId);
    Task<(bool Success, string Content, int id)> ExtendContractAsync(ExtendRentalDto extend,int customerId);
    Task<(bool Success, string Content, int id)> CloseContractAsync(RentalCloseDto request,int customerId);
    Task<bool> HasActiveRentalAsync(int carId,DateTime start,DateTime end);



  }
}
