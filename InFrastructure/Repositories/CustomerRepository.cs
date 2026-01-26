using ApplicationCore.Entities;
using ApplicationCore.Interfaces.Repositories;
using InFrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InFrastructure.Repositories
{
  public class CustomerRepository:ICustomerRepository
  {
    private readonly AppDbContext _context;

    public CustomerRepository(AppDbContext context)
    {
      _context=context;
    }

    public async Task<Customer?> GetByEmail(string Email)
    {
      return await _context.Customers
          .FirstOrDefaultAsync(x => x.Email==Email);
    }

    public IQueryable<Customer> GetAll()
    {
      return _context.Customers.AsQueryable();
    }

    public async Task<Customer?> GetByIdAsync(int id)
    {
      return await _context.Customers.FindAsync(id);
    }

    public async Task AddAsync(Customer entity)
    {
      await _context.Customers.AddAsync(entity);
    }

    public void Update(Customer entity)
    {
      _context.Customers.Update(entity);
    }

    public void Delete(Customer entity)
    {
      _context.Customers.Remove(entity);
    }
  }
}
