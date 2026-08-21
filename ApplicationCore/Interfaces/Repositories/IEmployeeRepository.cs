using ApplicationCore.Entities;

namespace ApplicationCore.Interfaces.Repositories
{
    public interface IEmployeeRepository : IGenericRepository<Employee>
    {
        Task<Employee?> GetByEmailAsync(string email);
        Task<IEnumerable<Employee>> GetAllAsync();
    }
}
