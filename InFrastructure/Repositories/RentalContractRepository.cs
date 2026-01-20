using ApplicationCore.Entities;
using ApplicationCore.Entities.Enums;
using ApplicationCore.Interfaces.Repositories;
using InFrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InFrastructure.Repositories
{
  public class RentalContractRepository:GenericRepository<RentalContract>, IRentalContractRepository
  {

    public RentalContractRepository(AppDbContext context) : base(context)
    {


    }

    public IQueryable<RentalContract?> GetOpenContracts()
    {
      return _dbSet.Where(r => r.Status==RentalContractStatus.Open);
    }

    public IQueryable<RentalContract> GetContractByCustomer(int customerId)
    {
      return _dbSet.Where(r => r.CustomerId==customerId);
    }

    public Task<bool> HasActiveRentalAsync(int carId,DateTime? start,DateTime? end)
    {
      return _context.RentalContracts.AnyAsync(r => r.CarId==carId&&r.Status==RentalContractStatus.Open&&(
        start<=r.EndDate
        &&end>=r.StartDate));
    }
  }
}
