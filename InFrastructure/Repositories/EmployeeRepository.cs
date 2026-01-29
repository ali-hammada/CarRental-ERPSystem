using ApplicationCore.Entities;
using ApplicationCore.Interfaces.Repositories;
using InFrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InFrastructure.Repositories
{
  public class EmployeeRepository:GenericRepository<Employees>, IEmployeeRepository
  {
    private readonly AppDbContext _context;

    public EmployeeRepository(AppDbContext context) : base(context)
    {
      _context=context;
    }

    public async Task<Employees?> GetByEmailAsync(string email)
    {
      return await _context.Employee
          .FirstOrDefaultAsync(x => x.Email==email);
    }
  }
}
