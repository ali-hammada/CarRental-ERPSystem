using ApplicationCore.Entities;

namespace ApplicationCore.Interfaces.Repositories
{
  public interface IRentalContractRepository:IGenericRepository<RentalContract>
  {
    Task<bool> HasActiveRentalAsync(int carId,DateTime? start,DateTime? end);

  }
}
