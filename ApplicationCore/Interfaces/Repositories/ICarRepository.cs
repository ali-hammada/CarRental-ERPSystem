using ApplicationCore.Entities;

namespace ApplicationCore.Interfaces.Repositories
{
  public interface ICarRepository:IGenericRepository<Car>
  {
    IQueryable<Car> GetAvailableCars();
  }
}
