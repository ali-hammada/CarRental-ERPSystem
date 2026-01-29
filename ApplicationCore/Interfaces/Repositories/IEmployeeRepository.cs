using ApplicationCore.Entities;

namespace ApplicationCore.Interfaces.Repositories
{
  public interface IEmployeeRepository:IGenericRepository<Employees>
  {
    Task<Employees?> GetByEmailAsync(string email);
  }
}
