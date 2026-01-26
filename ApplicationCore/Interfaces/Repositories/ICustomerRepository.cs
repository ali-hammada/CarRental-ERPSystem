using ApplicationCore.Entities;

namespace ApplicationCore.Interfaces.Repositories
{
  public interface ICustomerRepository:IGenericRepository<Customer>
  {
    Task<Customer?> GetByEmail(string Email);
  }
}
