using ApplicationCore.Entities;
using ApplicationCore.Interfaces.Repositories;

namespace ApplicationCore.Interfaces
{
  public interface IUnitOfWork
  {
    ICarRepository Cars { get; }
    IGenericRepository<Customer> Customers { get; }
    IGenericRepository<RentalContract> RentalContracts { get; }
    IGenericRepository<Payment> Payments { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();


  }
}
