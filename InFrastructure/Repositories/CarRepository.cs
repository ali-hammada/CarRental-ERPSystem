using ApplicationCore.Entities;
using ApplicationCore.Entities.Enums;
using ApplicationCore.Interfaces.Repositories;
using InFrastructure.Data;

namespace InFrastructure.Repositories
{
  public class CarRepository:GenericRepository<Car>, ICarRepository
  {

    public CarRepository(AppDbContext context) : base(context)
    {
    }

    public IQueryable<Car> GetAvailableCars()
    {
      return _dbSet.Where(c => c.Status==CarStatus.Available);
    }


  }
}
