using ApplicationCore.Entities;

namespace Application.Services.Interfaces
{
  public interface IEmployeeServices
  {
    Task<IEnumerable<Employees>> GetAllAsync();
    Task<Employees?> GetByIdAsync(int id);
    Task AddAsync(Employees employee);
    Task UpdateAsync(Employees employee);
    Task<bool> DeleteAsync(int id);
    Task<Employees?> GetByEmailAsync(string email);
  }
}
