using ApplicationCore.Entities;
using InFrastructure.Data;

namespace InFrastructure.Repositories
{
  public class CustomerRepository:GenericRepository<Customer>
  {
    public CustomerRepository(AppDbContext context) : base(context)
    {
    }
    public IQueryable<Customer> GetCustomersW_V_Licence()
    {
      return _dbSet.Where(c => c.LicenseExpiryDate>DateTime.Now);
    }

  }
}
