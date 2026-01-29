using ApplicationCore.Entities;
using ApplicationCore.Interfaces.Repositories;

namespace ApplicationCore.Interfaces
{
  public interface IUnitOfWork
  {
    ICarRepository Cars { get; }
    ICustomerRepository Customer { get; }
    IGenericRepository<Employees> Employee { get; }
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
    Task<int> SaveChangesAsync();
  }
}
