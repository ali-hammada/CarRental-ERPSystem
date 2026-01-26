using ApplicationCore.Interfaces.Repositories;

namespace ApplicationCore.Interfaces
{
  public interface IUnitOfWork
  {
    ICarRepository Cars { get; }


    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
    Task<int> SaveChangesAsync();
  }
}
