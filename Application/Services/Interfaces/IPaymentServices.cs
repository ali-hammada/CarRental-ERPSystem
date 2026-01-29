using ApplicationCore.Entities;
using ApplicationCore.Entities.Enums;


namespace Application.Services.Interfaces
{
  public interface IPaymentServices
  {
    Task<(bool success, string message, int PaymentId)> MakePaymentAsync(int rentalContractId,decimal amount,PaymentPurpose purpose,PaymentMethod method,int customerId);
    Task<(bool success, string message, decimal remaining)> GetRemainingAmountAsync(int rentalContractId,int customerId);
    Task<List<Payment>> GetContractPaymentsAsync(int rentalContractId,int customerId);
    Task<Payment?> GetPaymentByIdAsync(int paymentId,int customerId);
    Task<List<Payment>> GetAllEmployeesPaymentsAsync(int customerId);







  }
}
