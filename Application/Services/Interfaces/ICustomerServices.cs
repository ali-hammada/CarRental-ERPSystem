using ApplicationCore.Entities;

namespace Application.Services.Interfaces
{
  public interface ICustomerServices
  {

    Task<IEnumerable<Customer>> GetAllCustomersAsync();
    Task<Customer?> GetByIdAsync(int id);
    Task AddCustomerAsync(Customer customer);
    Task UpdateCustomerAsync(Customer customer);
    Task<bool> DeleteCustomerAsync(int id);
  }
}

