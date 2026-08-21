using ApplicationCore.Entities;
using ApplicationCore.Interfaces.Repositories;
using InFrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InFrastructure.Repositories
{
    public class EmployeeRepository : GenericRepository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(AppDbContext context) : base(context) { }

        public async Task<Employee?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(e => e.Email == email);
        }

        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }
    }
}
